using Microsoft.EntityFrameworkCore;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Entities;
using PantryCloud.SharedKernel.Identity;

namespace PantryCloud.Pantry.UnitTests;

internal class TestPantryDbContext(DbContextOptions<PantryDbContext> options, IUserContext? userContext = null) : PantryDbContext(options)
{
    private readonly IUserContext? _userContext = userContext;

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

        // Handle auditing manually for tests if user context is provided
        if (_userContext != null)
        {
            var userId = _userContext.UserId;
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (entry.Entity.CreatedBy == Guid.Empty)
                        {
                            entry.Entity.CreatedBy = userId;
                        }
                        if (entry.Entity.CreatedAt == default)
                        {
                            entry.Entity.CreatedAt = utcNow;
                        }
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedBy = userId;
                        entry.Entity.ModifiedAt = utcNow;
                        break;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

