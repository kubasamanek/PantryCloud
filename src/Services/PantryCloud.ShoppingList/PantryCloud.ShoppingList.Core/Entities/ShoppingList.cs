using PantryCloud.SharedKernel.Entities;

namespace PantryCloud.ShoppingList.Core.Entities;

public class ShoppingList : AuditableEntity
{
    public Guid HouseholdId { get; init; }
    public required string Name { get; set; }
    public ICollection<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();
}

