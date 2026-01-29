namespace PantryCloud.SharedKernel.Entities;

/// <summary>
/// Base class for entities that need auditing (tracking who created/modified and when).
/// Inherits from <see cref="BaseEntity"/> and adds auditing properties.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the ID of the user who created this entity.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified this entity.
    /// </summary>
    public Guid? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }
}



