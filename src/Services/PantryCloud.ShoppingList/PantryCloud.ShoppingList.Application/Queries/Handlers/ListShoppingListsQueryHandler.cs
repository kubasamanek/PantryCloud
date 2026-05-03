using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Queries.Handlers;

public class ListShoppingListsQueryHandler(IShoppingListManagementService shoppingListManagementService)
    : IRequestHandler<ListShoppingListsQuery, ErrorOr<ListShoppingListsResponseDto>>
{
    public async Task<ErrorOr<ListShoppingListsResponseDto>> Handle(ListShoppingListsQuery request, CancellationToken cancellationToken)
    {
        return await shoppingListManagementService.ListShoppingListsAsync(request.Request, cancellationToken);
    }
}


