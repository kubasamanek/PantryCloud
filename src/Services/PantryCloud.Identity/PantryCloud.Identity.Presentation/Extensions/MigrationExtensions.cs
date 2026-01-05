using Microsoft.EntityFrameworkCore;
using PantryCloud.Identity.Infrastructure.Persistence;

namespace PantryCloud.Identity.Presentation.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }}