namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Abstraction for message bus operations.
/// Allows services to publish events without being coupled to a specific message broker implementation.
/// Subscriptions are handled via MassTransit's IConsumer&lt;T&gt; pattern for proper DI scoping.
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publishes an event to the message bus.
    /// </summary>
    /// <typeparam name="T">The type of the event</typeparam>
    /// <param name="event">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IntegrationEvent;
}
