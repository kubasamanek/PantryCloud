using ErrorOr;
using MediatR;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Application.Commands;

public record LogoutCommand(LogoutRequestDto Request) : IRequest<ErrorOr<Unit>>;
