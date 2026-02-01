using DotNet.Testcontainers.Networks;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;
using Testcontainers.RabbitMq;

namespace PantryCloud.Notification.IntegrationTests.Infrastructure.Containers;

public static class RabbitMqContainerConfig
{
    public static RabbitMqBuilder Create(INetwork network) =>
        new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .WithUsername(Constants.RabbitMq.User)
            .WithPassword(Constants.RabbitMq.Password)
            .WithNetwork(network)
            .WithNetworkAliases(Constants.RabbitMq.Host)
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging);
}
