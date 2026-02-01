using MassTransit;

namespace PantryCloud.Notification.IntegrationTests.Infrastructure.Environment;

public sealed class EventPublisher : IAsyncDisposable
{
    private readonly IBusControl _bus;

    public EventPublisher(string rabbitHostPort)
    {
        var uri = new Uri($"amqp://{Constants.RabbitMq.User}:{Constants.RabbitMq.Password}@{rabbitHostPort}/");
        _bus = MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(uri, h =>
            {
                h.Username(Constants.RabbitMq.User);
                h.Password(Constants.RabbitMq.Password);
            });
        });
        _bus.StartAsync().GetAwaiter().GetResult();
    }

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class =>
        _bus.Publish(message, cancellationToken);

    public async ValueTask DisposeAsync() => await _bus.StopAsync();
}
