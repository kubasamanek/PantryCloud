using PantryCloud.Household.Core.Enums;

namespace PantryCloud.Household.Core.Entities;

public class HouseholdMember
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public Guid UserId { get; set; }
    public HouseholdRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}