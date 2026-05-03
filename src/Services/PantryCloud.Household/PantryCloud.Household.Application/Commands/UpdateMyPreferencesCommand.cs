using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands;

public record UpdateMyPreferencesCommand(UpdateMyPreferencesRequestDto Request) : IRequest<ErrorOr<UpdateMyPreferencesResponseDto>>;
