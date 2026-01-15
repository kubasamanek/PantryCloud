using MassTransit;

namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// MassTransit implementation of IMessageBus.
/// Wraps MassTransit's IPublishEndpoint for publishing integration events.
/// </summary>
public class MassTransitBus(IPublishEndpoint publishEndpoint) : IMessageBus
{
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IntegrationEvent
    {
        await publishEndpoint.Publish(@event, cancellationToken);
    }
}

