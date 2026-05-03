using ErrorOr;
using MediatR;

namespace PantryCloud.Identity.Application.Commands;

public record RevokeAllOtherSessionsCommand : IRequest<ErrorOr<Unit>>;
