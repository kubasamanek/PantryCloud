using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class LeaveHouseholdCommandHandler(
    IHouseholdManagementService householdManagementService,
    IOutboxWriter outboxWriter,
    ICorrelationIdProvider correlationIdProvider)
    : IRequestHandler<LeaveHouseholdCommand, ErrorOr<LeaveHouseholdResponseDto>>
{
    public async Task<ErrorOr<LeaveHouseholdResponseDto>> Handle(LeaveHouseholdCommand request, CancellationToken cancellationToken)
    {
        var result = await householdManagementService.LeaveHousehold(request.Request, cancellationToken);

        return await result.WriteToOutboxIfSuccessAsync(
            outboxWriter,
            (response, correlationId) => new MemberLeftHouseholdEvent
            {
                HouseholdId = response.HouseholdId!.Value,
                MemberId = response.MemberId!.Value,
                MemberEmail = response.MemberEmail,
                LeftAt = response.LeftAt!.Value,
                CorrelationId = correlationId
            },
            correlationIdProvider,
            cancellationToken);
    }
}