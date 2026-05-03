using ErrorOr;
using MediatR;

namespace PantryCloud.Identity.Application.Commands;

public record RevokeSessionCommand(Guid SessionId) : IRequest<ErrorOr<Unit>>;
