using PantryCloud.Household.Core.Enums;
using PantryCloud.SharedKernel.Entities;

namespace PantryCloud.Household.Core.Entities;

public class HouseholdMember : BaseEntity
{
    public Guid HouseholdId { get; set; }
    public Guid UserId { get; init; }
    public HouseholdRole Role { get; init; }
    public DateTime JoinedAt { get; init; }
}