using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Application.Commands.Handlers;

public class CreatePantryItemCommandHandler(IPantryManagementService pantryManagementService) : IRequestHandler<CreatePantryItemCommand, ErrorOr<CreatePantryItemResponseDto>>
{
    public async Task<ErrorOr<CreatePantryItemResponseDto>> Handle(CreatePantryItemCommand request, CancellationToken cancellationToken)
    {
        return await pantryManagementService.CreatePantryItemAsync(request.Request, cancellationToken);
    }
}

