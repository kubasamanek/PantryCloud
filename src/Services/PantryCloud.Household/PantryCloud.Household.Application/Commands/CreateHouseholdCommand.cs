using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands;

public record CreateHouseholdCommand(CreateHouseholdRequestDto Request) : IRequest<ErrorOr<CreateHouseholdResponseDto>>;