namespace PantryCloud.SharedKernel.Entities;

/// <summary>
/// Interface for entities that have a unique identifier.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    Guid Id { get; }
}



