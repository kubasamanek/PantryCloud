using MassTransit;
using NSubstitute;
using PantryCloud.SharedKernel.Messaging;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Messaging;

public class MassTransitBusTests
{
    [Fact]
    public async Task PublishAsync_ShouldDelegateToPublishEndpoint()
    {
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var bus = new PantryCloud.SharedKernel.Messaging.MassTransitBus(publishEndpoint);
        var @event = new TestPublishEvent();

        await bus.PublishAsync(@event);

        await publishEndpoint.Received(1).Publish(
            Arg.Is<TestPublishEvent>(e => e.Id == @event.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_ShouldPassCancellationTokenToEndpoint()
    {
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var bus = new PantryCloud.SharedKernel.Messaging.MassTransitBus(publishEndpoint);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        await bus.PublishAsync(new TestPublishEvent(), token);

        await publishEndpoint.Received(1).Publish(
            Arg.Any<TestPublishEvent>(),
            token);
    }

    private class TestPublishEvent : IntegrationEvent { }
}
