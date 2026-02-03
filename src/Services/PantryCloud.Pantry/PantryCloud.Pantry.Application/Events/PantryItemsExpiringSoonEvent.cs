using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Application.Events;

public record ExpiringItemDto(
    Guid ItemId,
    string Name,
    DateTime ExpirationDate,
    int DaysUntilExpiry);

public class PantryItemsExpiringSoonEvent : IntegrationEvent
{
    public Guid HouseholdId { get; init; }
    public List<ExpiringItemDto> Items { get; init; } = [];
}
