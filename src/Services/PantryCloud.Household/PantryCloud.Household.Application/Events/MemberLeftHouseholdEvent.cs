using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Household.Application.Events;

public class MemberLeftHouseholdEvent : IntegrationEvent
{
    public Guid HouseholdId { get; init; }
    public Guid MemberId { get; init; }
    public string? MemberEmail { get; init; }
    public DateTime LeftAt { get; init; }
}

