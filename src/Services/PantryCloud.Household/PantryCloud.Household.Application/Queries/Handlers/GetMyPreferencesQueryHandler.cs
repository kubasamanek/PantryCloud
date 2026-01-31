using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Queries.Handlers;

public class GetMyPreferencesQueryHandler(IPreferencesService preferencesService)
    : IRequestHandler<GetMyPreferencesQuery, ErrorOr<GetMyPreferencesResponseDto>>
{
    public async Task<ErrorOr<GetMyPreferencesResponseDto>> Handle(GetMyPreferencesQuery request, CancellationToken cancellationToken)
    {
        return await preferencesService.GetMyPreferencesAsync(cancellationToken);
    }
}
