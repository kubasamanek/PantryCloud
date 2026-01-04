namespace PantryCloud.Household.Core.Entities;

public class HouseholdInvitation
{
    public Guid Id { get; init; }
    public required Guid HouseholdId { get; init; }
    public required string Code { get; init; }
    public required string Email { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public DateTime? UsedAt { get; set; }
    
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsUsed => UsedAt.HasValue;
}