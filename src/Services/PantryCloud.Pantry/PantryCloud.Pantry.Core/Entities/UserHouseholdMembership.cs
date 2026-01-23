namespace PantryCloud.Pantry.Core.Entities;

public class UserHouseholdMembership
{
    public Guid UserId { get; init; }
    public Guid HouseholdId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    
    public bool IsActive => LeftAt == null;
}

