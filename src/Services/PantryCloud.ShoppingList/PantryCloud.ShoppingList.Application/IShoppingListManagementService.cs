using ErrorOr;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application;

public interface IShoppingListManagementService
{
    Task<ErrorOr<CreateShoppingListResponseDto>> CreateShoppingListAsync(CreateShoppingListRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<DeleteShoppingListResponseDto>> DeleteShoppingListAsync(DeleteShoppingListRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<GetShoppingListResponseDto>> GetShoppingListAsync(GetShoppingListRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<ListShoppingListsResponseDto>> ListShoppingListsAsync(ListShoppingListsRequestDto request, CancellationToken cancellationToken);
    
    Task<ErrorOr<AddShoppingListItemResponseDto>> AddShoppingListItemAsync(Guid listId, AddShoppingListItemRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<AddShoppingListItemsBatchResponseDto>> AddShoppingListItemsBatchAsync(Guid listId, AddShoppingListItemsBatchRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<UpdateShoppingListItemResponseDto>> UpdateShoppingListItemAsync(Guid listId, Guid itemId, UpdateShoppingListItemRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<DeleteShoppingListItemResponseDto>> DeleteShoppingListItemAsync(DeleteShoppingListItemRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<CheckShoppingListItemResponseDto>> CheckShoppingListItemAsync(CheckShoppingListItemRequestDto request, CancellationToken cancellationToken);
}

