using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands;

public record UpdateMyProfileCommand(UpdateMyProfileRequestDto Request) : IRequest<ErrorOr<UpdateMyProfileResponseDto>>;
