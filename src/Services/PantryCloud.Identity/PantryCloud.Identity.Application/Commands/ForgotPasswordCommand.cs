using ErrorOr;
using MediatR;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Application.Commands;

public record ForgotPasswordCommand(ForgotPasswordRequestDto Request) : IRequest<ErrorOr<ForgotPasswordResponseDto>>;