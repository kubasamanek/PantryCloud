using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Queries.Handlers;

public class GetCurrentHouseholdQueryHandler(IHouseholdManagementService householdManagementService) : IRequestHandler<GetCurrentHouseholdQuery, ErrorOr<GetCurrentHouseholdResponseDto>>
{

    public async Task<ErrorOr<GetCurrentHouseholdResponseDto>> Handle(GetCurrentHouseholdQuery request, CancellationToken cancellationToken)
    {
        return await householdManagementService.GetCurrentHousehold(cancellationToken);
    }
}