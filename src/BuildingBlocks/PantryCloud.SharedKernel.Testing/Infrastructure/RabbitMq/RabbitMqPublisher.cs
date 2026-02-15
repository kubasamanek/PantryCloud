using MassTransit;

namespace PantryCloud.SharedKernel.Testing.Infrastructure.RabbitMq;

/// <summary>
/// Publishes messages to RabbitMQ for integration tests. 
/// (e.g. publish a domain event so the Notification service creates a notification).
/// </summary>
public sealed class RabbitMqPublisher : IAsyncDisposable
{
    private readonly IBusControl _bus;

    /// <summary>
    /// Creates a publisher that connects to RabbitMQ at the given host and port.
    /// </summary>
    public RabbitMqPublisher(
         string host,
         int port,
         string user = RabbitMqTestOptions.DefaultUser,
         string password = RabbitMqTestOptions.DefaultPassword)
    {
        var uri = new Uri($"amqp://{Uri.EscapeDataString(user)}:{Uri.EscapeDataString(password)}@{host}:{port}/");
        _bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(uri, h =>
            {
                h.Username(user);
                h.Password(password);
            });
        });
        _bus.StartAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Creates a publisher using "host:port"
    /// </summary>
    public static RabbitMqPublisher CreateFromHostPort(
        string hostPort,
        string user = RabbitMqTestOptions.DefaultUser,
        string password = RabbitMqTestOptions.DefaultPassword)
    {
        var parts = hostPort.IndexOf(':') >= 0
            ? hostPort.Split(':', 2)
            : [hostPort, RabbitMqTestOptions.AmqpPort.ToString()];
        var host = parts[0];
        var port = int.Parse(parts.Length > 1 ? parts[1] : RabbitMqTestOptions.AmqpPort.ToString());
        return new RabbitMqPublisher(host, port, user, password);
    }

    /// <summary>
    /// Publishes a message to RabbitMQ. Consumers in the service under test will receive it (same exchange/contract as production).
    /// </summary>
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class =>
        _bus.Publish(message, cancellationToken);

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await _bus.StopAsync();
}
