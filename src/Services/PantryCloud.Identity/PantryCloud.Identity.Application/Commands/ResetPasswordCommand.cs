using ErrorOr;
using MediatR;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Application.Commands;

public record ResetPasswordCommand(ResetPasswordRequestDto Request) : IRequest<ErrorOr<ResetPasswordResponseDto>>;