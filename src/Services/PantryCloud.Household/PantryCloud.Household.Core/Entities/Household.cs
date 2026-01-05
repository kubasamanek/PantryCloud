namespace PantryCloud.Household.Core.Entities;

public class Household
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;

    public ICollection<HouseholdMember> Members { get; init; } = new List<HouseholdMember>();
    public ICollection<HouseholdInvitation> Invitations { get; init; } = new List<HouseholdInvitation>();
}