using MassTransit;
using Microsoft.Extensions.Logging;

namespace PantryCloud.SharedKernel.Messaging;

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

    protected abstract Task Handle(TEvent @event, ConsumeContext context);
}
