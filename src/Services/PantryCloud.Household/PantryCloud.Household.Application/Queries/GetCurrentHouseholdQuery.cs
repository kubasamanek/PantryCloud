using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Queries;

public record GetCurrentHouseholdQuery(GetCurrentHouseholdRequestDto Request) : IRequest<ErrorOr<GetCurrentHouseholdResponseDto>>;