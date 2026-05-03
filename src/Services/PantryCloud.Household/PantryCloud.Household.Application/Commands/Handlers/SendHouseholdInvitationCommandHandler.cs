using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class SendHouseholdInvitationCommandHandler(IInvitationService invitationService)
    : IRequestHandler<SendHouseholdInvitationCommand, ErrorOr<SendHouseholdInvitationResponseDto>>
{
    public async Task<ErrorOr<SendHouseholdInvitationResponseDto>> Handle(SendHouseholdInvitationCommand request, CancellationToken cancellationToken)
    {
        return await invitationService.SendHouseholdInvitation(request.Request, cancellationToken);
    }
}