using Microsoft.EntityFrameworkCore;
using PantryCloud.Household.Infrastructure.Persistence;

namespace PantryCloud.Household.Presentation.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HouseholdDbContext>();
        await db.Database.MigrateAsync();
    }}