namespace PantryCloud.Web.Services.ShoppingList;

public interface IShoppingListApi
{
    Task<ListShoppingListsResult> ListListsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<GetShoppingListResult> GetListAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CreateShoppingListResult> CreateListAsync(CreateShoppingListRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteListAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AddShoppingListItemResponse?> AddItemAsync(Guid listId, AddShoppingListItemRequest request, CancellationToken cancellationToken = default);
    Task<ShoppingListItemUpdateResult> UpdateItemAsync(Guid listId, Guid itemId, UpdateShoppingListItemRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken = default);
    Task<CheckShoppingListItemResponse?> CheckItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken = default);
    Task<AddShoppingListItemsBatchResponse?> AddItemsBatchAsync(Guid listId, AddShoppingListItemsBatchRequest request, CancellationToken cancellationToken = default);
}
