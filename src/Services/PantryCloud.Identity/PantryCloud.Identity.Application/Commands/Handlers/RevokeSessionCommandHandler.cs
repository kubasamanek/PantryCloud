using ErrorOr;
using MediatR;

namespace PantryCloud.Identity.Application.Commands.Handlers;

public sealed class RevokeSessionCommandHandler(IAuthService authService)
    : IRequestHandler<RevokeSessionCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        return await authService.RevokeSessionAsync(request.SessionId, cancellationToken);
    }
}
