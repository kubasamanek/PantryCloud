using ErrorOr;
using PantryCloud.SharedKernel.Correlation;

namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Extension methods for publishing integration events with correlation ID support.
/// </summary>
public static class EventPublishingExtensions
{
    /// <summary>
    /// Publishes an integration event if the result is successful, automatically including correlation ID.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <typeparam name="TEvent">The integration event type.</typeparam>
    /// <param name="result">The ErrorOr result.</param>
    /// <param name="messageBus">The message bus.</param>
    /// <param name="eventFactory">Factory function to create the event from the result and correlation ID.</param>
    /// <param name="correlationIdProvider">The correlation ID provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The original result.</returns>
    public static async Task<ErrorOr<TResult>> PublishIfSuccessAsync<TResult, TEvent>(
        this ErrorOr<TResult> result,
        IMessageBus messageBus,
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
        await messageBus.PublishAsync(@event, cancellationToken);

        return result;
    }
}
