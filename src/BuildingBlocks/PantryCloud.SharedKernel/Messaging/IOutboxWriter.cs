namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Writes integration events to the transactional outbox within the current EF Core change tracker.
/// The <see cref="OutboxRelayWorker{TDbContext}"/> is responsible for publishing them to the broker.
/// Persistence is handled by the UnitOfWork pipeline behavior.
/// </summary>
public interface IOutboxWriter
{
    Task WriteAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
