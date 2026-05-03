using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PantryCloud.SharedKernel.Entities;
using PantryCloud.SharedKernel.Identity;

namespace PantryCloud.SharedKernel.Persistence;

/// <summary>
/// Interceptor that automatically sets auditing properties (CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
/// on entities that inherit from <see cref="AuditableEntity"/>.
/// </summary>
public class AuditableEntityInterceptor(IUserContext userContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context == null)
            return;

        var entries = context.ChangeTracker.Entries<AuditableEntity>();

        // Only access UserId if there are actually AuditableEntity entries to update
        // This prevents exceptions during operations like registration/login where there's no authenticated user
        if (!entries.Any())
            return;

        var userId = userContext.UserId;
        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.CreatedAt = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedBy = userId;
                    entry.Entity.ModifiedAt = utcNow;
                    break;
            }
        }
    }
}
