using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Application.Commands.Handlers;

public class DeletePantryItemCommandHandler(IPantryManagementService pantryManagementService) : IRequestHandler<DeletePantryItemCommand, ErrorOr<DeletePantryItemResponseDto>>
{
    public async Task<ErrorOr<DeletePantryItemResponseDto>> Handle(DeletePantryItemCommand request, CancellationToken cancellationToken)
    {
        return await pantryManagementService.DeletePantryItemAsync(request.Request, cancellationToken);
    }
}

