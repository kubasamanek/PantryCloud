using ErrorOr;
using MediatR;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Application.Commands;

public record RegisterCommand(RegisterRequestDto Request) : IRequest<ErrorOr<RegisterResponseDto>>;