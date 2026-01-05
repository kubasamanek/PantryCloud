using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands;

public record AcceptHouseholdInvitationCommand(AcceptHouseholdInvitationRequestDto Request) : IRequest<ErrorOr<AcceptHouseholdInvitationResponseDto>>;