using ErrorOr;
using PantryCloud.SharedKernel.Correlation;

namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Extension methods for writing integration events to the transactional outbox with correlation ID support.
/// </summary>
public static class EventPublishingExtensions
{
    /// <summary>
    /// Writes an integration event to the transactional outbox if the result is successful.
    /// The event is NOT published directly to the broker; the <see cref="OutboxRelayWorker{TDbContext}"/>
    /// will publish it asynchronously after the UnitOfWork behavior commits the transaction.
    /// </summary>
    public static async Task<ErrorOr<TResult>> WriteToOutboxIfSuccessAsync<TResult, TEvent>(
        this ErrorOr<TResult> result,
        IOutboxWriter outboxWriter,
        Func<TResult, string?, TEvent> eventFactory,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        if (result.IsError)
        {
            return result;
        }

        var correlationId = correlationIdProvider.GetCorrelationId();
        var @event = eventFactory(result.Value, correlationId);
        await outboxWriter.WriteAsync(@event, cancellationToken);

        return result;
    }
}
