using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands.Handlers;

public class CreateShoppingListCommandHandler(IShoppingListManagementService shoppingListManagementService) 
    : IRequestHandler<CreateShoppingListCommand, ErrorOr<CreateShoppingListResponseDto>>
{
    public async Task<ErrorOr<CreateShoppingListResponseDto>> Handle(CreateShoppingListCommand request, CancellationToken cancellationToken)
    {
        return await shoppingListManagementService.CreateShoppingListAsync(request.Request, cancellationToken);
    }
}


