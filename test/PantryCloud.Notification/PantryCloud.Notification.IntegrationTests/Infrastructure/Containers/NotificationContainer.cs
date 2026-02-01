using DotNet.Testcontainers.Builders;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;
using DotNet.Testcontainers.Networks;
using DotNet.Testcontainers.Images;

namespace PantryCloud.Notification.IntegrationTests.Infrastructure.Containers;

public static class NotificationContainer
{
    public static ContainerBuilder Create(IFutureDockerImage image, INetwork network, string connectionString, string rabbitHost, string issuerSigningKeyBase64)
    {
        var env = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = Constants.Environment.Testing,
            ["ASPNETCORE_HTTP_PORTS"] = Constants.Notification.HttpPorts,
            ["ConnectionStrings__DefaultConnection"] = connectionString,
            ["Messaging__RabbitMQ__Host"] = rabbitHost,
            ["Messaging__RabbitMQ__Username"] = Constants.RabbitMq.User,
            ["Messaging__RabbitMQ__Password"] = Constants.RabbitMq.Password,
            ["Jwt__IssuerSigningKey"] = issuerSigningKeyBase64,
            ["Jwt__Issuer"] = Constants.Jwt.Issuer,
            ["Jwt__Audience"] = Constants.Jwt.Audience
        };
        var builder = new ContainerBuilder()
            .WithImage(image)
            .WithNetwork(network)
            .WithNetworkAliases(Constants.Notification.NetworkAlias)
            .WithPortBinding(Constants.Notification.Port, true)
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/health").ForPort(Constants.Notification.Port)));
        foreach (var (k, v) in env)
            builder = builder.WithEnvironment(k, v);
        return builder;
    }
}
