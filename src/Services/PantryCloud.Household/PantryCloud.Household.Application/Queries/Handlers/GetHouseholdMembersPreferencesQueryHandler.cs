using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Queries.Handlers;

public class GetHouseholdMembersPreferencesQueryHandler(IPreferencesService preferencesService)
    : IRequestHandler<GetHouseholdMembersPreferencesQuery, ErrorOr<GetHouseholdMembersPreferencesResponseDto>>
{
    public async Task<ErrorOr<GetHouseholdMembersPreferencesResponseDto>> Handle(
        GetHouseholdMembersPreferencesQuery request,
        CancellationToken cancellationToken)
    {
        return await preferencesService.GetHouseholdMembersPreferencesAsync(cancellationToken);
    }
}
