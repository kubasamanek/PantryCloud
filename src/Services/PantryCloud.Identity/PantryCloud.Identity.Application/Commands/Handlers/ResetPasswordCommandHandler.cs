using ErrorOr;
using MediatR;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Application.Commands.Handlers;

public class ResetPasswordCommandHandler(IAuthService authService) : IRequestHandler<ResetPasswordCommand, ErrorOr<ResetPasswordResponseDto>>
{
    public async Task<ErrorOr<ResetPasswordResponseDto>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        return await authService.ResetPasswordAsync(request.Request, cancellationToken);
    }
}