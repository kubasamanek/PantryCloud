using ErrorOr;
using MediatR;

namespace PantryCloud.Identity.Application.Commands.Handlers;

public sealed class RevokeAllOtherSessionsCommandHandler(IAuthService authService)
    : IRequestHandler<RevokeAllOtherSessionsCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(RevokeAllOtherSessionsCommand request, CancellationToken cancellationToken)
    {
        return await authService.RevokeAllOtherSessionsAsync(cancellationToken);
    }
}
