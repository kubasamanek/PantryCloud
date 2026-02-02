using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands;

public record KickMemberCommand(KickMemberRequestDto Request) : IRequest<ErrorOr<KickMemberResponseDto>>;
