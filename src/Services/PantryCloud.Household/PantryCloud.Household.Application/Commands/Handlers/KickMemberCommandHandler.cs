using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class KickMemberCommandHandler(
    IHouseholdManagementService householdManagementService,
    IMessageBus messageBus,
    ICorrelationIdProvider correlationIdProvider)
    : IRequestHandler<KickMemberCommand, ErrorOr<KickMemberResponseDto>>
{
    public async Task<ErrorOr<KickMemberResponseDto>> Handle(KickMemberCommand request, CancellationToken cancellationToken)
    {
        var result = await householdManagementService.KickMemberAsync(request.Request, cancellationToken);

        return await result.PublishIfSuccessAsync(
            messageBus,
            (response, correlationId) => new MemberLeftHouseholdEvent
            {
                HouseholdId = response.HouseholdId,
                MemberId = response.KickedUserId,
                MemberEmail = null,
                LeftAt = response.KickedAt,
                CorrelationId = correlationId
            },
            correlationIdProvider,
            cancellationToken);
    }
}
