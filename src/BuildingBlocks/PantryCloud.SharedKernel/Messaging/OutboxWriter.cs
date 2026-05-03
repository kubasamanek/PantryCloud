using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.SharedKernel.Outbox;

namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// EF Core-backed implementation of <see cref="IOutboxWriter"/>.
/// Serializes the event and inserts an <see cref="OutboxMessage"/> row using the scoped DbContext.
/// </summary>
public class OutboxWriter<TDbContext>(TDbContext dbContext, ILogger<OutboxWriter<TDbContext>> logger) : IOutboxWriter
    where TDbContext : DbContext
{
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public async Task WriteAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        var outboxMessage = new OutboxMessage
        {
            EventType = @event.GetType().AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(@event, @event.GetType(), _serializerOptions),
            OccurredAt = @event.OccurredAt
        };

        await dbContext.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken);

        logger.LogInformation(
            "Outbox<{DbContext}>: queued {EventType} (OutboxId: {OutboxId}, EventId: {EventId})",
            typeof(TDbContext).Name,
            typeof(TEvent).Name,
            outboxMessage.Id,
            @event.Id);
    }
}
