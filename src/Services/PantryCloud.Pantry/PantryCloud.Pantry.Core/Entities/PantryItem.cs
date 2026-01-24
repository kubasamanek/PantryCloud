using PantryCloud.Pantry.Core.Enums;
using PantryCloud.SharedKernel.Entities;

namespace PantryCloud.Pantry.Core.Entities;

public class PantryItem : AuditableEntity
{
    public Guid HouseholdId { get; init; }
    public required string Name { get; set; }
    public decimal Quantity { get; set; }
    public Unit Unit { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}

