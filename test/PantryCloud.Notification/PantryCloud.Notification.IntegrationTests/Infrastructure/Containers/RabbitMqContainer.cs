using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
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
            .WithCreateParameterModifier(p =>
            {
                p.HostConfig ??= new HostConfig();
                p.HostConfig.LogConfig = new LogConfig { Type = "json-file" };
            });
}
