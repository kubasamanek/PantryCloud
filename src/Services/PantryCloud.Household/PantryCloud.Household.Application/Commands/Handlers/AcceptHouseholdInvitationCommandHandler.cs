using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class AcceptHouseholdInvitationCommandHandler(IInvitationService invitationService) : IRequestHandler<AcceptHouseholdInvitationCommand, ErrorOr<AcceptHouseholdInvitationResponseDto>>
{
    public async Task<ErrorOr<AcceptHouseholdInvitationResponseDto>> Handle(AcceptHouseholdInvitationCommand request, CancellationToken cancellationToken)
    {
        return await invitationService.AcceptHouseholdInvitation(request.Request, cancellationToken);
    }
}