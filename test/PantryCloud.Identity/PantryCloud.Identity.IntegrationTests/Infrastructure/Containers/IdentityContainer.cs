using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

namespace PantryCloud.Identity.IntegrationTests.Infrastructure.Containers;

public static class IdentityContainer
{
    public static ContainerBuilder Create(IFutureDockerImage image, INetwork network, string connectionString, string secretsHostPath)
    {
        var env = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = IntegrationConstants.Environment.Testing,
            ["ASPNETCORE_HTTP_PORTS"] = IntegrationConstants.Identity.HttpPorts,
            ["ConnectionStrings__DefaultConnection"] = connectionString,
            ["Jwt__PrivateKeyPath"] = "/app/secrets/private.pem",
            ["Jwt__PublicKeyPath"] = "/app/secrets/public.pem",
            ["App__SendEmails"] = "false"
        };

        var builder = new ContainerBuilder()
            .WithImage(image)
            .WithNetwork(network)
            .WithNetworkAliases(IntegrationConstants.Identity.NetworkAlias)
            .WithPortBinding(IntegrationConstants.Identity.Port, true)
            .WithBindMount(secretsHostPath, "/app/secrets")
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/health").ForPort(IntegrationConstants.Identity.Port)));
        foreach (var (k, v) in env)
            builder = builder.WithEnvironment(k, v);
        return builder;
    }
}
