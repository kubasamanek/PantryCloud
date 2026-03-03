using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.SharedKernel.Outbox;

namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Base class for MassTransit consumers.
/// Provides structured logging, correlation ID enrichment, and inbox-level idempotency for consumers
/// that supply a DbContext. Duplicate messages (same broker MessageId) are acknowledged and skipped.
/// </summary>
/// <typeparam name="TEvent">The type of the integration event.</typeparam>
/// <typeparam name="TDbContext">The type of the DbContext (optional).</typeparam>
public abstract class ConsumerBase<TEvent, TDbContext> : IConsumer<TEvent>
    where TEvent : IntegrationEvent
    where TDbContext : class
{
    /// <summary>Gets the DbContext for database operations (null if not provided).</summary>
    protected TDbContext? DbContext { get; }

    /// <summary>Gets the logger for the consumer.</summary>
    protected ILogger Logger { get; }

    /// <summary>Initializes a new instance of the <see cref="ConsumerBase{TEvent, TDbContext}"/> class without DbContext.</summary>
    protected ConsumerBase(ILogger logger)
    {
        Logger = logger;
        DbContext = null;
    }

    /// <summary>Initializes a new instance of the <see cref="ConsumerBase{TEvent, TDbContext}"/> class with DbContext.</summary>
    protected ConsumerBase(TDbContext dbContext, ILogger logger)
    {
        DbContext = dbContext;
        Logger = logger;
    }

    /// <summary>Consumes the integration event with inbox idempotency and structured logging.</summary>
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var @event = context.Message;

        using var scope = Logger.BeginScope(new Dictionary<string, object>
        {
            ["EventId"] = @event.Id,
            ["EventName"] = typeof(TEvent).Name,
            ["CorrelationId"] = @event.CorrelationId ?? context.CorrelationId?.ToString() ?? string.Empty
        });

        Logger.LogDebug(
            "Consuming {EventName} - EventId: {EventId}, CorrelationId: {CorrelationId}",
            typeof(TEvent).Name,
            @event.Id,
            @event.CorrelationId ?? context.CorrelationId?.ToString() ?? string.Empty);

        try
        {
            if (DbContext is DbContext dbContext)
            {
                var brokerId = context.MessageId ?? Guid.NewGuid();

                var alreadyProcessed = await dbContext.Set<ProcessedInboxMessage>()
                    .AnyAsync(m => m.MessageId == brokerId, context.CancellationToken);

                if (alreadyProcessed)
                {
                    Logger.LogDebug(
                        "Skipping duplicate {EventName} - MessageId: {MessageId} already processed",
                        typeof(TEvent).Name,
                        brokerId);
                    return;
                }

                await HandleAsync(@event, context);

                dbContext.Set<ProcessedInboxMessage>().Add(new ProcessedInboxMessage
                {
                    MessageId = brokerId,
                    EventType = typeof(TEvent).AssemblyQualifiedName!,
                    ProcessedAt = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync(context.CancellationToken);
            }
            else
            {
                await HandleAsync(@event, context);
            }

            Logger.LogInformation(
                "Successfully consumed {EventName} - EventId: {EventId}",
                typeof(TEvent).Name,
                @event.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Error consuming {EventName} - EventId: {EventId}, CorrelationId: {CorrelationId}",
                typeof(TEvent).Name,
                @event.Id,
                @event.CorrelationId ?? context.CorrelationId?.ToString() ?? string.Empty);
            throw;
        }
    }

    /// <summary>Handles the integration event.</summary>
    protected abstract Task HandleAsync(TEvent @event, ConsumeContext context);
}
