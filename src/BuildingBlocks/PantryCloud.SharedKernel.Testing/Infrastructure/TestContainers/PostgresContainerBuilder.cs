using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;

namespace PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

/// <summary>
/// Shared Postgres container builder for integration tests.
/// </summary>
public static class PostgresContainerBuilder
{
    private const string DefaultUser = "admin";
    private const string DefaultPassword = "password";
    private const string DefaultDatabase = "postgres";
    private const string DefaultHostAlias = "postgres";

    public static PostgreSqlBuilder Create(
        INetwork network,
        string hostAlias = DefaultHostAlias,
        string user = DefaultUser,
        string password = DefaultPassword,
        string database = DefaultDatabase)
    {
        return new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase(database)
            .WithUsername(user)
            .WithPassword(password)
            .WithNetwork(network)
            .WithNetworkAliases(hostAlias)
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging);
    }
}
