using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Household.Application.Events;

public class PreferenceAddedEvent : IntegrationEvent
{
    public Guid HouseholdId { get; init; }
    public Guid UserId { get; init; }
}
