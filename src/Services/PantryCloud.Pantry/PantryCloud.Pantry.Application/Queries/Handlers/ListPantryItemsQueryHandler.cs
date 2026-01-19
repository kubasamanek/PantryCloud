using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Application.Queries.Handlers;

public class ListPantryItemsQueryHandler(IPantryManagementService pantryManagementService) : IRequestHandler<ListPantryItemsQuery, ErrorOr<ListPantryItemsResponseDto>>
{
    public async Task<ErrorOr<ListPantryItemsResponseDto>> Handle(ListPantryItemsQuery request, CancellationToken cancellationToken)
    {
        return await pantryManagementService.ListPantryItemsAsync(request.Request, cancellationToken);
    }
}

