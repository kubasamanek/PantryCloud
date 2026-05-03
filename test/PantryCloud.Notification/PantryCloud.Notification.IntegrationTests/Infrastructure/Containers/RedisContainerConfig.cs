using DotNet.Testcontainers.Networks;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;
using Testcontainers.Redis;

namespace PantryCloud.Notification.IntegrationTests.Infrastructure.Containers;

public static class RedisContainerConfig
{
    public static RedisBuilder Create(INetwork network) =>
        new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithNetwork(network)
            .WithNetworkAliases(Constants.Redis.Host)
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging);
}
