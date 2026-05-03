using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands;

public record SendHouseholdInvitationCommand(SendHouseholdInvitationRequestDto Request) : IRequest<ErrorOr<SendHouseholdInvitationResponseDto>>;