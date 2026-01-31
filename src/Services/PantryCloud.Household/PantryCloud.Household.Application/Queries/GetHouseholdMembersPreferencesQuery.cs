using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Queries;

public record GetHouseholdMembersPreferencesQuery(GetHouseholdMembersPreferencesRequestDto Request)
    : IRequest<ErrorOr<GetHouseholdMembersPreferencesResponseDto>>;
