namespace PantryCloud.Web.Services.Pantry;

public interface IPantryApi
{
    Task<ListPantryItemsResult> ListItemsAsync(string? category, string? searchTerm, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PantryItemDto?> GetItemAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CreatePantryItemResponse?> CreateItemAsync(CreatePantryItemRequest request, CancellationToken cancellationToken = default);
    Task<PantryUpdateResult> UpdateItemAsync(Guid id, UpdatePantryItemRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default);
}
