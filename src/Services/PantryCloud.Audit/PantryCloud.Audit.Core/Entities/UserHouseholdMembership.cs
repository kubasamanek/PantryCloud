namespace PantryCloud.Audit.Core.Entities;

public class UserHouseholdMembership
{
    public Guid UserId { get; set; }
    public Guid HouseholdId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}
