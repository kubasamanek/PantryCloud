using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands.Handlers;

public class AddShoppingListItemCommandHandler(IShoppingListManagementService shoppingListManagementService) 
    : IRequestHandler<AddShoppingListItemCommand, ErrorOr<AddShoppingListItemResponseDto>>
{
    public async Task<ErrorOr<AddShoppingListItemResponseDto>> Handle(AddShoppingListItemCommand request, CancellationToken cancellationToken)
    {
        return await shoppingListManagementService.AddShoppingListItemAsync(request.ListId, request.Request, cancellationToken);
    }
}


