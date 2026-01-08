using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PantryCloud.SharedKernel.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync<TContext>(this IHost host) 
        where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        await db.Database.MigrateAsync();
    }
}

