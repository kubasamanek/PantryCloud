using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands.Handlers;

public class UpdateShoppingListItemCommandHandler(IShoppingListManagementService shoppingListManagementService) 
    : IRequestHandler<UpdateShoppingListItemCommand, ErrorOr<UpdateShoppingListItemResponseDto>>
{
    public async Task<ErrorOr<UpdateShoppingListItemResponseDto>> Handle(UpdateShoppingListItemCommand request, CancellationToken cancellationToken)
    {
        return await shoppingListManagementService.UpdateShoppingListItemAsync(request.ListId, request.ItemId, request.Request, cancellationToken);
    }
}


