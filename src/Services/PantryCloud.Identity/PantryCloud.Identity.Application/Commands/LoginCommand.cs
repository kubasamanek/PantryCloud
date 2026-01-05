using ErrorOr;
using MediatR;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Application.Commands;

public record LoginCommand(LoginRequestDto Request) : IRequest<ErrorOr<LoginResponseDto>>;