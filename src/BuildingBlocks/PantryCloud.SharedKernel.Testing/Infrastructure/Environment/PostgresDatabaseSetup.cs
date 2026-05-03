using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace PantryCloud.SharedKernel.Testing.Infrastructure.Environment;

/// <summary>
/// Shared PostgreSQL database setup for integration tests.
/// Creates databases and applies EF Core migrations before starting service containers.
/// </summary>
public static class PostgresDatabaseSetup
{
    /// <summary>
    /// Creates a new database in the Postgres container.
    /// </summary>
    /// <param name="postgres">The running Postgres container.</param>
    /// <param name="databaseName">Name of the database to create.</param>
    /// <param name="user">Postgres username.</param>
    /// <param name="defaultDatabase">Default database to connect to for the CREATE command.</param>
    public static async Task CreateDatabaseAsync(
        PostgreSqlContainer postgres,
        string databaseName,
        string user = "admin",
        string defaultDatabase = "postgres")
    {
        await postgres.ExecAsync(["psql", "-U", user, "-d", defaultDatabase, "-c", $"CREATE DATABASE {databaseName};"]);
        await Task.Delay(500);
    }

    /// <summary>
    /// Applies EF Core migrations for the given DbContext to the specified database.
    /// Uses the host-mapped port so migrations run from the test process.
    /// </summary>
    public static async Task ApplyMigrationsAsync<TContext>(
        PostgreSqlContainer postgres,
        string databaseName,
        string user = "admin",
        string password = "password",
        int port = 5432)
        where TContext : DbContext
    {
        var hostPort = postgres.GetMappedPublicPort(port);
        var connStr = $"Host=127.0.0.1;Port={hostPort};Database={databaseName};Username={user};Password={password}";
        var options = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(connStr, o => o.MigrationsAssembly(typeof(TContext).Assembly.FullName))
            .Options;
        var context = (TContext)Activator.CreateInstance(typeof(TContext), options)!;
        await using (context)
        {
            await context.Database.MigrateAsync();
        }
    }
}
