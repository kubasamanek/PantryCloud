using PantryCloud.SharedKernel.Messaging;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Messaging;

public class IntegrationEventTests
{
    [Fact]
    public void NewEvent_ShouldHaveNonDefaultId()
    {
        var @event = new TestEvent();

        @event.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void NewEvent_ShouldHaveOccurredAtCloseToUtcNow()
    {
        var before = DateTime.UtcNow;
        var @event = new TestEvent();
        var after = DateTime.UtcNow;

        @event.OccurredAt.ShouldBeGreaterThanOrEqualTo(before);
        @event.OccurredAt.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public void NewEvent_ShouldHaveNullCorrelationIdByDefault()
    {
        var @event = new TestEvent();

        @event.CorrelationId.ShouldBeNull();
    }

    [Fact]
    public void NewEvent_ShouldAcceptCorrelationId()
    {
        var @event = new TestEvent { CorrelationId = Constants.Outbox.CorrelationId };

        @event.CorrelationId.ShouldBe(Constants.Outbox.CorrelationId);
    }

    [Fact]
    public void TwoEvents_ShouldHaveDifferentIds()
    {
        var first = new TestEvent();
        var second = new TestEvent();

        first.Id.ShouldNotBe(second.Id);
    }

    private class TestEvent : IntegrationEvent { }
}
