using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PantryCloud.Audit.Infrastructure.Consumers;
using PantryCloud.Household.Application.Events;
using Shouldly;

namespace PantryCloud.Audit.UnitTests.Consumers;

public class PreferenceAddedAuditConsumerTests
{
    [Fact]
    public async Task Consume_ShouldCreateAuditEntry_WhenEventReceived()
    {
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldCreateAuditEntry_WhenEventReceived));
        var logger = TestHelper.MockLogger<PreferenceAddedAuditConsumer>();
        var consumer = new PreferenceAddedAuditConsumer(db, logger);

        var @event = new PreferenceAddedEvent
        {
            Id = eventId,
            HouseholdId = householdId,
            UserId = userId,
            OccurredAt = occurredAt,
            CorrelationId = Constants.Audit.CorrelationId
        };

        var context = Substitute.For<ConsumeContext<PreferenceAddedEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var entries = await db.HouseholdAuditEntries.ToListAsync();
        entries.Count.ShouldBe(1);
        entries[0].ActionType.ShouldBe(Constants.Audit.ActionPreferenceAdded);
        entries[0].EntityType.ShouldBe(Constants.Audit.EntityPreference);
        entries[0].EntityId.ShouldBe(userId);
        entries[0].UserId.ShouldBe(userId);
    }
}
