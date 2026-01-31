using PantryCloud.SharedKernel.Entities;
using PantryCloud.SharedKernel.Enums;
using PantryCloud.ShoppingList.Core.Enums;

namespace PantryCloud.ShoppingList.Core.Entities;

public class ShoppingListItem : AuditableEntity
{
    public Guid ShoppingListId { get; init; }
    public required string Name { get; set; }
    public decimal Quantity { get; set; } = 1;
    public Unit Unit { get; set; }
    public bool IsChecked { get; set; }
    public Guid? CheckedBy { get; set; }
    public DateTime? CheckedAt { get; set; }
    public ItemSource Source { get; set; } = ItemSource.Manual;
    public byte[] RowVersion { get; set; } = null!;
    
    public ShoppingList ShoppingList { get; set; } = null!;
}


