using ErrorOr;
using MediatR;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Application.Commands.Handlers;

public class VerifyEmailCommandHandler(IAuthService authService) : IRequestHandler<VerifyEmailCommand, ErrorOr<VerifyEmailResponseDto>>
{
    public async Task<ErrorOr<VerifyEmailResponseDto>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        return await authService.VerifyEmailAsync(request.Request, cancellationToken);
    }
}