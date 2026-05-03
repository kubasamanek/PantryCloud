using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Application.Events;

public class PantryItemDeletedEvent : IntegrationEvent
{
    public Guid HouseholdId { get; init; }
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public Guid InitiatedByUserId { get; init; }
}
