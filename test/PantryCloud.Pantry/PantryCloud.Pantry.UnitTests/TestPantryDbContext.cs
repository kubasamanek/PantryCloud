using Microsoft.EntityFrameworkCore;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Infrastructure.Persistence;

namespace PantryCloud.Pantry.UnitTests;

internal class TestPantryDbContext(DbContextOptions<PantryDbContext> options) : PantryDbContext(options)
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In-memory database doesn't support IsRowVersion(), set RowVersion manually
        foreach (var entry in ChangeTracker.Entries<PantryItem>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (entry.Entity.RowVersion == null || entry.Entity.RowVersion.Length == 0)
                {
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

