using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PantryCloud.SharedKernel.Behaviors;

/// <summary>
/// MediatR pipeline behavior that flushes any remaining unsaved EF Core changes
/// after a command handler succeeds.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(
    DbContext dbContext,
    ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        if (response is IErrorOr { IsError: true } || !dbContext.ChangeTracker.HasChanges())
        {
            return response;
        }

        logger.LogDebug(
            "UnitOfWork: committing remaining changes for {RequestType}",
            typeof(TRequest).Name);

        await dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }
}
