using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Queries.Handlers;

public class GetHouseholdByUserIdQueryHandler(IHouseholdManagementService householdManagementService)
    : IRequestHandler<GetHouseholdByUserIdQuery, ErrorOr<GetHouseholdByUserIdResponseDto>>
{
    public async Task<ErrorOr<GetHouseholdByUserIdResponseDto>> Handle(GetHouseholdByUserIdQuery request, CancellationToken cancellationToken)
    {
        return await householdManagementService.GetHouseholdByUserId(request.Request, cancellationToken);
    }
}

