using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands;

public record LeaveHouseholdCommand(LeaveHouseholdRequestDto Request) : IRequest<ErrorOr<LeaveHouseholdResponseDto>>;