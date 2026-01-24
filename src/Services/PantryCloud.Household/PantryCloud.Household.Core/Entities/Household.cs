using PantryCloud.SharedKernel.Entities;

namespace PantryCloud.Household.Core.Entities;

public class Household : BaseEntity
{
    public string Name { get; init; } = null!;

    public ICollection<HouseholdMember> Members { get; init; } = new List<HouseholdMember>();
    public ICollection<HouseholdInvitation> Invitations { get; init; } = new List<HouseholdInvitation>();
}