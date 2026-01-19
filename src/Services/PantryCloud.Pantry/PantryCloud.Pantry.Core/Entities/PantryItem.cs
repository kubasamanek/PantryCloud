using PantryCloud.Pantry.Core.Enums;

namespace PantryCloud.Pantry.Core.Entities;

public class PantryItem
{
    public Guid Id { get; init; }
    public Guid HouseholdId { get; init; }
    public required string Name { get; set; }
    public decimal Quantity { get; set; }
    public Unit Unit { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

