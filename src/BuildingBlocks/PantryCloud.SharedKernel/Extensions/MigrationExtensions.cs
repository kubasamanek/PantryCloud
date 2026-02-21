using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PantryCloud.SharedKernel.Extensions;

public static class MigrationExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations at startup.
    /// </summary>
    public static async Task ApplyMigrationsAsync<TContext>(this IHost host)
        where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("PantryCloud.Migrations");
        await db.Database.MigrateAsync();
        logger?.LogInformation("EF Core migrations applied for {Context}", typeof(TContext).Name);
    }
}
