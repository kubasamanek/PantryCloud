namespace PantryCloud.SharedKernel.Entities;

/// <summary>
/// Base class for all entities with a unique identifier.
/// </summary>
public abstract class BaseEntity : IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier of the entity.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
}


