using ErrorOr;
using MediatR;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Application.Commands.Handlers;

public sealed class ListSessionsCommandHandler(IAuthService authService)
    : IRequestHandler<ListSessionsCommand, ErrorOr<ListSessionsResponseDto>>
{
    public async Task<ErrorOr<ListSessionsResponseDto>> Handle(ListSessionsCommand request, CancellationToken cancellationToken)
    {
        return await authService.ListSessionsAsync(cancellationToken);
    }
}
