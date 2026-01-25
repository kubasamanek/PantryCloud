using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands.Handlers;

public class DeleteShoppingListItemCommandHandler(IShoppingListManagementService shoppingListManagementService) 
    : IRequestHandler<DeleteShoppingListItemCommand, ErrorOr<DeleteShoppingListItemResponseDto>>
{
    public async Task<ErrorOr<DeleteShoppingListItemResponseDto>> Handle(DeleteShoppingListItemCommand request, CancellationToken cancellationToken)
    {
        return await shoppingListManagementService.DeleteShoppingListItemAsync(request.Request, cancellationToken);
    }
}

