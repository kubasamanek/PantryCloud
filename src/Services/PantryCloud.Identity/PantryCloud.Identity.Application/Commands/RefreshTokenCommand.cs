using ErrorOr;
using MediatR;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Application.Commands;

public record RefreshTokenCommand(RefreshTokenRequestDto Request) : IRequest<ErrorOr<RefreshTokenResponseDto>>;
