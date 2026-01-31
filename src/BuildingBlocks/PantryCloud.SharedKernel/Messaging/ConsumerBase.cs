using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Base class for MassTransit consumers.
/// Provides common logging and error handling patterns.
/// Optionally supports DbContext for database operations.
/// </summary>
/// <typeparam name="TEvent">The type of the integration event.</typeparam>
/// <typeparam name="TDbContext">The type of the DbContext (optional).</typeparam>
public abstract class ConsumerBase<TEvent, TDbContext> : IConsumer<TEvent>
    where TEvent : IntegrationEvent
    where TDbContext : class
{
    /// <summary>
    /// Gets the DbContext for database operations (null if not provided).
    /// </summary>
    protected TDbContext? DbContext { get; }

    /// <summary>
    /// Gets the logger for the consumer.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumerBase{TEvent, TDbContext}"/> class without DbContext.
    /// </summary>
    /// <param name="logger">The logger.</param>
    protected ConsumerBase(ILogger logger)
    {
        Logger = logger;
        DbContext = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumerBase{TEvent, TDbContext}"/> class with DbContext.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    protected ConsumerBase(TDbContext dbContext, ILogger logger)
    {
        DbContext = dbContext;
        Logger = logger;
    }

    /// <summary>
    /// Consumes the integration event.
    /// </summary>
    /// <param name="context">The consume context.</param>
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var @event = context.Message;

        using var scope = Logger.BeginScope(new Dictionary<string, object>
        {
            ["EventId"] = @event.Id,
            ["EventName"] = typeof(TEvent).Name,
            ["CorrelationId"] = @event.CorrelationId ?? context.CorrelationId?.ToString() ?? string.Empty
        });

        Logger.LogInformation(
            "Consuming {EventName} - EventId: {EventId}, CorrelationId: {CorrelationId}",
            typeof(TEvent).Name,
            @event.Id,
            @event.CorrelationId ?? context.CorrelationId?.ToString() ?? string.Empty);

        try
        {
            await HandleAsync(@event, context);

            // Save changes if DbContext is provided
            if (DbContext is DbContext dbContext)
            {
                await dbContext.SaveChangesAsync(context.CancellationToken);
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

    /// <summary>
    /// Handles the integration event. Implement this method to provide the specific handling logic.
    /// </summary>
    /// <param name="event">The integration event.</param>
    /// <param name="context">The consume context.</param>
    protected abstract Task HandleAsync(TEvent @event, ConsumeContext context);
}
