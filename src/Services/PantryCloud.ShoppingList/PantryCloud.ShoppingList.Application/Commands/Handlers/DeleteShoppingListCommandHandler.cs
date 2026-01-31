using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands.Handlers;

public class DeleteShoppingListCommandHandler(IShoppingListManagementService shoppingListManagementService) 
    : IRequestHandler<DeleteShoppingListCommand, ErrorOr<DeleteShoppingListResponseDto>>
{
    public async Task<ErrorOr<DeleteShoppingListResponseDto>> Handle(DeleteShoppingListCommand request, CancellationToken cancellationToken)
    {
        return await shoppingListManagementService.DeleteShoppingListAsync(request.Request, cancellationToken);
    }
}


