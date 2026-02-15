using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using PantryCloud.Household.IntegrationTests.Constants;
using PantryCloud.SharedKernel.Testing.Infrastructure.RabbitMq;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

namespace PantryCloud.Household.IntegrationTests.Infrastructure.Containers;

public static class HouseholdContainer
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
            ["ASPNETCORE_HTTP_PORTS"] = IntegrationConstants.Household.HttpPorts,
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
            .WithNetworkAliases(IntegrationConstants.Household.NetworkAlias)
            .WithPortBinding(IntegrationConstants.Household.Port, true)
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/health").ForPort(IntegrationConstants.Household.Port)));

        foreach (var (k, v) in env)
        {
            builder = builder.WithEnvironment(k, v);
        }
        return builder;
    }
}