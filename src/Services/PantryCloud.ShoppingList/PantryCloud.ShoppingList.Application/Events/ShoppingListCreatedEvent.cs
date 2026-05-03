using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.ShoppingList.Application.Events;

public class ShoppingListCreatedEvent : IntegrationEvent
{
    public Guid HouseholdId { get; init; }
    public Guid ShoppingListId { get; init; }
    public string ShoppingListName { get; init; } = string.Empty;
    public Guid CreatedByUserId { get; init; }
}
