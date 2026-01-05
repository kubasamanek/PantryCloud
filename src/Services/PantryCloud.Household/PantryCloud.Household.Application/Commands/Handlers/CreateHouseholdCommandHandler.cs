using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class CreateHouseholdCommandHandler(IHouseholdManagementService householdManagementService) : IRequestHandler<CreateHouseholdCommand, ErrorOr<CreateHouseholdResponseDto>>
{
    public async Task<ErrorOr<CreateHouseholdResponseDto>> Handle(CreateHouseholdCommand request, CancellationToken cancellationToken)
    {
        return await householdManagementService.CreateHousehold(request.Request, cancellationToken);
    }
}