using Microsoft.EntityFrameworkCore;
using PantryCloud.Notification.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PantryCloud.Notification.IntegrationTests.Infrastructure.Environment;

public static class DatabaseSetup
{
    public static async Task CreateDatabaseAsync(PostgreSqlContainer postgres)
    {
        await postgres.ExecAsync(["psql", "-U", Constants.Postgres.User, "-d", Constants.Postgres.DefaultDatabase, "-c", $"CREATE DATABASE {Constants.Postgres.NotificationDatabase};"]);
        await Task.Delay(500);
    }

    public static async Task ApplyMigrationsAsync(PostgreSqlContainer postgres)
    {
        var connStr = $"Host={postgres.Hostname};Port={postgres.GetMappedPublicPort(Constants.Postgres.Port)};Database={Constants.Postgres.NotificationDatabase};Username={Constants.Postgres.User};Password={Constants.Postgres.Password}";
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql(connStr, o => o.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName))
            .Options;
        await using var context = new NotificationDbContext(options);
        await context.Database.MigrateAsync();
    }
}
