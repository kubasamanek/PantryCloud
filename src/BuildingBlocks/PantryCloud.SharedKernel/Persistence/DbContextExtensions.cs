using Microsoft.EntityFrameworkCore;
using PantryCloud.SharedKernel.Entities;

namespace PantryCloud.SharedKernel.Persistence;

/// <summary>
/// Extension methods for DbContext to provide common operations.
/// </summary>
public static class  DbContextExtensions
{
    /// <summary>
    /// Finds an entity by ID, or returns null if not found.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="context">The database context.</param>
    /// <param name="id">The entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity if found, otherwise null.</returns>
    public static async Task<TEntity?> FindByIdAsync<TEntity>(
        this DbContext context,
        Guid id,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        return await context.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    /// <summary>
    /// Checks if an entity exists by ID.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="context">The database context.</param>
    /// <param name="id">The entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the entity exists, otherwise false.</returns>
    public static async Task<bool> ExistsAsync<TEntity>(
        this DbContext context,
        Guid id,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        return await context.Set<TEntity>().AnyAsync(e => e.Id == id, cancellationToken);
    }
}



