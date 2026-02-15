using DotNet.Testcontainers.Networks;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;
using Testcontainers.PostgreSql;

namespace PantryCloud.Pantry.IntegrationTests.Infrastructure.Containers;

public static class PostgresContainer
{
    public static PostgreSqlBuilder Create(INetwork network) =>
        PostgresContainerBuilder.Create(network);
}
