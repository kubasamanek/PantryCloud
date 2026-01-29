using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands.Handlers;

public class CheckShoppingListItemCommandHandler(IShoppingListManagementService shoppingListManagementService) 
    : IRequestHandler<CheckShoppingListItemCommand, ErrorOr<CheckShoppingListItemResponseDto>>
{
    public async Task<ErrorOr<CheckShoppingListItemResponseDto>> Handle(CheckShoppingListItemCommand request, CancellationToken cancellationToken)
    {
        return await shoppingListManagementService.CheckShoppingListItemAsync(request.Request, cancellationToken);
    }
}


