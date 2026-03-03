using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PantryCloud.SharedKernel.Messaging;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Messaging;

public class IntegrationEventHandlerTests
{
    [Fact]
    public async Task Consume_ShouldCallHandle_WithEventFromContext()
    {
        var handler = new TrackingHandler();
        var @event = new TestHandledEvent { CorrelationId = Constants.Outbox.CorrelationId };
        var context = BuildContext(@event);

        await handler.Consume(context);

        handler.ReceivedEvent.ShouldNotBeNull();
        handler.ReceivedEvent.Id.ShouldBe(@event.Id);
    }

    [Fact]
    public async Task Consume_ShouldRethrowException_WhenHandlerThrows()
    {
        var handler = new ThrowingHandler();
        var context = BuildContext(new TestHandledEvent());

        await Should.ThrowAsync<InvalidOperationException>(() => handler.Consume(context));
    }

    [Fact]
    public async Task Consume_ShouldIncrementCallCount_OnEachInvocation()
    {
        var handler = new TrackingHandler();

        await handler.Consume(BuildContext(new TestHandledEvent()));
        await handler.Consume(BuildContext(new TestHandledEvent()));

        handler.HandleCallCount.ShouldBe(2);
    }

    private static ConsumeContext<TestHandledEvent> BuildContext(TestHandledEvent @event)
    {
        var context = Substitute.For<ConsumeContext<TestHandledEvent>>();
        context.Message.Returns(@event);
        context.CorrelationId.Returns((Guid?)null);
        return context;
    }
}

public class TestHandledEvent : IntegrationEvent { }

internal class TrackingHandler()
    : IntegrationEventHandler<TestHandledEvent>(NullLogger<TrackingHandler>.Instance)
{
    public TestHandledEvent? ReceivedEvent { get; private set; }
    public int HandleCallCount { get; private set; }

    protected override Task Handle(TestHandledEvent @event, ConsumeContext context)
    {
        ReceivedEvent = @event;
        HandleCallCount++;
        return Task.CompletedTask;
    }
}

internal class ThrowingHandler()
    : IntegrationEventHandler<TestHandledEvent>(NullLogger<ThrowingHandler>.Instance)
{
    protected override Task Handle(TestHandledEvent @event, ConsumeContext context) =>
        throw new InvalidOperationException("Handler failed");
}
