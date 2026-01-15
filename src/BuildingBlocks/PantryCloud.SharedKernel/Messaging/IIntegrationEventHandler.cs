using MassTransit;

namespace PantryCloud.SharedKernel.Messaging;

public interface IIntegrationEventHandler<in TEvent> : IConsumer<TEvent>
    where TEvent : IntegrationEvent;
