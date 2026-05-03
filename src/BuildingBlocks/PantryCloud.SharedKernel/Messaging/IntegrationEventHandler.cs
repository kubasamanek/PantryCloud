using MassTransit;
using Microsoft.Extensions.Logging;

namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Base class for integration event handlers with built-in logging and error handling.
/// Provides a simpler abstraction over MassTransit's IConsumer for handling integration events.
/// </summary>
/// <typeparam name="TEvent">The type of the integration event.</typeparam>
public abstract class IntegrationEventHandler<TEvent>(ILogger logger) : IIntegrationEventHandler<TEvent>
    where TEvent : IntegrationEvent
{
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["EventId"] = context.Message.Id,
            ["EventName"] = typeof(TEvent).Name,
            ["CorrelationId"] = context.Message.CorrelationId ?? context.CorrelationId?.ToString() ?? string.Empty
        });

        logger.LogInformation("Handling integration event: {EventName}", typeof(TEvent).Name);

        try
        {
            await Handle(context.Message, context);
            logger.LogInformation("Successfully handled integration event: {EventName}", typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling integration event: {EventName}", typeof(TEvent).Name);
            throw;
        }
    }

    /// <summary>
    /// Handles the integration event. Implement this method to provide the specific handling logic.
    /// </summary>
    /// <param name="event">The integration event.</param>
    /// <param name="context">The consume context.</param>
    protected abstract Task Handle(TEvent @event, ConsumeContext context);
}
