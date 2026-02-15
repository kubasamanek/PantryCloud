namespace PantryCloud.SharedKernel.Testing.Infrastructure.RabbitMq;

/// <summary>
/// Default RabbitMQ connection options for integration tests.
/// </summary>
public static class RabbitMqTestOptions
{
    public const string DefaultUser = "admin";
    public const string DefaultPassword = "password";
    public const int AmqpPort = 5672;
}
