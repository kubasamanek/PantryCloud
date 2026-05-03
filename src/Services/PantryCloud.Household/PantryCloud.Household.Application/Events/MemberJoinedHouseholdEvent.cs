using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Household.Application.Events;

public class MemberJoinedHouseholdEvent : IntegrationEvent
{
    public Guid HouseholdId { get; init; }
    public Guid NewMemberId { get; init; }
    public string? MemberEmail { get; init; }
    public DateTime JoinedAt { get; init; }
}

