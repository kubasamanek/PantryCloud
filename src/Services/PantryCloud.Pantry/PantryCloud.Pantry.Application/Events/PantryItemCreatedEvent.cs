using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Application.Events;

public class PantryItemCreatedEvent : IntegrationEvent
{
    public Guid HouseholdId { get; init; }
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public Guid UserId { get; init; }
}
