using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands.Handlers;

public class AddShoppingListItemsBatchCommandHandler(IShoppingListManagementService shoppingListManagementService) 
    : IRequestHandler<AddShoppingListItemsBatchCommand, ErrorOr<AddShoppingListItemsBatchResponseDto>>
{
    public async Task<ErrorOr<AddShoppingListItemsBatchResponseDto>> Handle(AddShoppingListItemsBatchCommand request, CancellationToken cancellationToken)
    {
        return await shoppingListManagementService.AddShoppingListItemsBatchAsync(request.ListId, request.Request, cancellationToken);
    }
}


