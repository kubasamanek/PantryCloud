using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;
using Testcontainers.RabbitMq;

namespace PantryCloud.SharedKernel.Testing.Infrastructure.RabbitMq;

/// <summary>
/// Shared RabbitMQ container builder for integration tests.
/// </summary>
public static class RabbitMqContainerBuilder
{
    /// <summary>
    /// Builds a RabbitMQ container with default test credentials (no network).
    /// </summary>
    public static RabbitMqContainer Build(
        string user = RabbitMqTestOptions.DefaultUser,
        string password = RabbitMqTestOptions.DefaultPassword)
    {
        return new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .WithUsername(user)
            .WithPassword(password)
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging)
            .Build();
    }

    /// <summary>
    /// Builds a RabbitMQ container attached to a network (e.g. for multi-container tests).
    /// </summary>
    public static RabbitMqBuilder Create(
        DotNet.Testcontainers.Networks.INetwork network,
        string networkAlias = "rabbitmq",
        string user = RabbitMqTestOptions.DefaultUser,
        string password = RabbitMqTestOptions.DefaultPassword)
    {
        return new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .WithUsername(user)
            .WithPassword(password)
            .WithNetwork(network)
            .WithNetworkAliases(networkAlias)
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging);
    }
}
