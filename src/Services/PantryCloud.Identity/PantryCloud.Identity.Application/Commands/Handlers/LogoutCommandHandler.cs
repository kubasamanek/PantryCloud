using ErrorOr;
using MediatR;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Application.Commands.Handlers;

public sealed class LogoutCommandHandler(IAuthService authService)
    : IRequestHandler<LogoutCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await authService.LogoutAsync(request.Request, cancellationToken);
    }
}
