using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Application.Queries.Handlers;

public class GetPantryItemQueryHandler(IPantryManagementService pantryManagementService) : IRequestHandler<GetPantryItemQuery, ErrorOr<GetPantryItemResponseDto>>
{
    public async Task<ErrorOr<GetPantryItemResponseDto>> Handle(GetPantryItemQuery request, CancellationToken cancellationToken)
    {
        return await pantryManagementService.GetPantryItemAsync(request.Request, cancellationToken);
    }
}

