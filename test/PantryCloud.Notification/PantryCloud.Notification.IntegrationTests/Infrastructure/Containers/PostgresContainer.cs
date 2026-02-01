using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;

namespace PantryCloud.Notification.IntegrationTests.Infrastructure.Containers;

public static class PostgresContainer
{
    public static PostgreSqlBuilder Create(INetwork network) =>
        new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase(Constants.Postgres.DefaultDatabase)
            .WithUsername(Constants.Postgres.User)
            .WithPassword(Constants.Postgres.Password)
            .WithNetwork(network)
            .WithNetworkAliases(Constants.Postgres.Host)
            .WithCreateParameterModifier(p =>
            {
                p.HostConfig ??= new HostConfig();
                p.HostConfig.LogConfig = new LogConfig { Type = "json-file" };
            });
}
