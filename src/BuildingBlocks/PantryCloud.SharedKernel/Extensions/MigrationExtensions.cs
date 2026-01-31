using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PantryCloud.SharedKernel.Extensions;

/// <summary>
/// Extension methods for applying Entity Framework Core migrations.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Applies pending migrations for the specified DbContext.
    /// Typically called during application startup in development environments.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext.</typeparam>
    /// <param name="host">The application host.</param>
    public static async Task ApplyMigrationsAsync<TContext>(this IHost host) 
        where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        await db.Database.MigrateAsync();
    }
}
