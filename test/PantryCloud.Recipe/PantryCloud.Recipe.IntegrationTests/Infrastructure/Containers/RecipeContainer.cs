using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using PantryCloud.Recipe.IntegrationTests.Constants;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

namespace PantryCloud.Recipe.IntegrationTests.Infrastructure.Containers;

public static class RecipeContainer
{
    public static ContainerBuilder Create(
        IFutureDockerImage image,
        INetwork network,
        string mongoConnectionString,
        string jwtIssuerSigningKeyBase64)
    {
        var env = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = IntegrationConstants.Environment.Development,
            ["ASPNETCORE_HTTP_PORTS"] = IntegrationConstants.Recipe.HttpPorts,
            ["ConnectionStrings__DefaultConnection"] = mongoConnectionString,
            ["ApiConfiguration__MongoDb__DatabaseName"] = IntegrationConstants.Mongo.DatabaseName,
            ["Jwt__IssuerSigningKey"] = jwtIssuerSigningKeyBase64,
            ["Jwt__Issuer"] = TestConstants.Jwt.Issuer,
            ["Jwt__Audience"] = TestConstants.Jwt.Audience
        };

        var builder = new ContainerBuilder()
            .WithImage(image)
            .WithNetwork(network)
            .WithNetworkAliases(IntegrationConstants.Recipe.NetworkAlias)
            .WithPortBinding(IntegrationConstants.Recipe.Port, true)
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/health").ForPort(IntegrationConstants.Recipe.Port)));

        foreach (var (k, v) in env)
        {
            builder = builder.WithEnvironment(k, v);
        }
        return builder;
    }
}
