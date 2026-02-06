using Microsoft.EntityFrameworkCore;
using PantryCloud.ShoppingList.Core.Entities;
using PantryCloud.ShoppingList.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Entities;
using PantryCloud.SharedKernel.Identity;

namespace PantryCloud.ShoppingList.UnitTests;

internal class TestShoppingListDbContext(DbContextOptions<ShoppingListDbContext> options, IUserContext? userContext = null)
    : ShoppingListDbContext(options)
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ShoppingListItem>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (entry.Entity.RowVersion == null || entry.Entity.RowVersion.Length == 0)
                {
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
                }
            }
        }

        if (userContext != null)
        {
            var userId = userContext.UserId;
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (entry.Entity.CreatedBy == Guid.Empty)
                            entry.Entity.CreatedBy = userId;
                        if (entry.Entity.CreatedAt == default)
                            entry.Entity.CreatedAt = utcNow;
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
