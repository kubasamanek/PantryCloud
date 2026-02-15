using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Queries.Handlers;

public class GetShoppingListQueryHandler(IShoppingListManagementService shoppingListManagementService)
    : IRequestHandler<GetShoppingListQuery, ErrorOr<GetShoppingListResponseDto>>
{
    public async Task<ErrorOr<GetShoppingListResponseDto>> Handle(GetShoppingListQuery request, CancellationToken cancellationToken)
    {
        return await shoppingListManagementService.GetShoppingListAsync(request.Request, cancellationToken);
    }
}


