using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Queries;

public record GetMyPreferencesQuery(GetMyPreferencesRequestDto Request) : IRequest<ErrorOr<GetMyPreferencesResponseDto>>;
