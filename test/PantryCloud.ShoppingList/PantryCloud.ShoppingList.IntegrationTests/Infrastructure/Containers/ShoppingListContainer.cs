using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using PantryCloud.SharedKernel.Testing.Infrastructure.RabbitMq;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;
using PantryCloud.ShoppingList.IntegrationTests.Constants;

namespace PantryCloud.ShoppingList.IntegrationTests.Infrastructure.Containers;

public static class ShoppingListContainer
{
    public static ContainerBuilder Create(
        IFutureDockerImage image,
        INetwork network,
        string connectionString,
        string rabbitMqHost,
        string jwtIssuerSigningKeyBase64)
    {
        var env = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = IntegrationConstants.Environment.Testing,
            ["ASPNETCORE_HTTP_PORTS"] = IntegrationConstants.ShoppingList.HttpPorts,
            ["ConnectionStrings__DefaultConnection"] = connectionString,
            ["Messaging__RabbitMQ__Host"] = rabbitMqHost,
            ["Messaging__RabbitMQ__Username"] = RabbitMqTestOptions.DefaultUser,
            ["Messaging__RabbitMQ__Password"] = RabbitMqTestOptions.DefaultPassword,
            ["Jwt__IssuerSigningKey"] = jwtIssuerSigningKeyBase64,
            ["Jwt__Issuer"] = TestConstants.Jwt.Issuer,
            ["Jwt__Audience"] = TestConstants.Jwt.Audience
        };

        var builder = new ContainerBuilder()
            .WithImage(image)
            .WithNetwork(network)
            .WithNetworkAliases(IntegrationConstants.ShoppingList.NetworkAlias)
            .WithPortBinding(IntegrationConstants.ShoppingList.Port, true)
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/health").ForPort(IntegrationConstants.ShoppingList.Port)));

        foreach (var (k, v) in env)
        {
            builder = builder.WithEnvironment(k, v);
        }
        return builder;
    }
}
