using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Application.Commands.Handlers;

public class UpdatePantryItemCommandHandler(IPantryManagementService pantryManagementService) : IRequestHandler<UpdatePantryItemCommand, ErrorOr<UpdatePantryItemResponseDto>>
{
    public async Task<ErrorOr<UpdatePantryItemResponseDto>> Handle(UpdatePantryItemCommand request, CancellationToken cancellationToken)
    {
        return await pantryManagementService.UpdatePantryItemAsync(request.Id, request.Request, cancellationToken);
    }
}

