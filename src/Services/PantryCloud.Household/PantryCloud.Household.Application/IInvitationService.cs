using ErrorOr;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application;

public interface IInvitationService
{
    Task<ErrorOr<SendHouseholdInvitationResponseDto>> SendHouseholdInvitation(SendHouseholdInvitationRequestDto request,
        CancellationToken cancellationToken);

    Task<ErrorOr<AcceptHouseholdInvitationResponseDto>> AcceptHouseholdInvitation(AcceptHouseholdInvitationRequestDto request,
        CancellationToken cancellationToken);
}