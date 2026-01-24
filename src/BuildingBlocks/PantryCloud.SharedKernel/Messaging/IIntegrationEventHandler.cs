using MassTransit;

namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Marker interface for integration event handlers.
/// Extends MassTransit's IConsumer to provide a domain-specific abstraction.
/// </summary>
/// <typeparam name="TEvent">The type of the integration event.</typeparam>
public interface IIntegrationEventHandler<in TEvent> : IConsumer<TEvent>
    where TEvent : IntegrationEvent;
