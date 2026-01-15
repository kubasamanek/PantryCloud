using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class AcceptHouseholdInvitationCommandHandler(
    IInvitationService invitationService,
    IMessageBus messageBus) 
    : IRequestHandler<AcceptHouseholdInvitationCommand, ErrorOr<AcceptHouseholdInvitationResponseDto>>
{
    public async Task<ErrorOr<AcceptHouseholdInvitationResponseDto>> Handle(AcceptHouseholdInvitationCommand request, CancellationToken cancellationToken)
    {
        var result = await invitationService.AcceptHouseholdInvitation(request.Request, cancellationToken);

        return await result.PublishIfSuccessAsync(
            messageBus,
            response => new MemberJoinedHouseholdEvent
            {
                HouseholdId = response.HouseholdId!.Value,
                NewMemberId = response.MemberId!.Value,
                MemberEmail = response.MemberEmail,
                JoinedAt = response.JoinedAt!.Value
            },
            cancellationToken);
    }
}