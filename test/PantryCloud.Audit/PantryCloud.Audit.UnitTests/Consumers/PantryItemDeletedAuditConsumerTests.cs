using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PantryCloud.Audit.Infrastructure.Consumers;
using PantryCloud.Pantry.Application.Events;
using Shouldly;

namespace PantryCloud.Audit.UnitTests.Consumers;

public class PantryItemDeletedAuditConsumerTests
{
    [Fact]
    public async Task Consume_ShouldCreateAuditEntry_WhenEventReceived()
    {
        var householdId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldCreateAuditEntry_WhenEventReceived));
        var logger = TestHelper.MockLogger<PantryItemDeletedAuditConsumer>();
        var consumer = new PantryItemDeletedAuditConsumer(db, logger);

        var @event = new PantryItemDeletedEvent
        {
            Id = eventId,
            HouseholdId = householdId,
            ItemId = itemId,
            ItemName = Constants.Audit.ItemName,
            InitiatedByUserId = userId,
            OccurredAt = occurredAt,
            CorrelationId = Constants.Audit.CorrelationId
        };

        var context = Substitute.For<ConsumeContext<PantryItemDeletedEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var entries = await db.HouseholdAuditEntries.ToListAsync();
        entries.Count.ShouldBe(1);
        entries[0].HouseholdId.ShouldBe(householdId);
        entries[0].ActionType.ShouldBe(Constants.Audit.ActionDeleted);
        entries[0].EntityType.ShouldBe(Constants.Audit.EntityPantryItem);
        entries[0].EntityId.ShouldBe(itemId);
        entries[0].UserId.ShouldBe(userId);
        entries[0].EventId.ShouldBe(eventId);
        entries[0].Payload!.ShouldContain(Constants.Audit.ItemName);
    }

    [Fact]
    public async Task Consume_ShouldSkip_WhenEventAlreadyProcessed()
    {
        var householdId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldSkip_WhenEventAlreadyProcessed));
        db.HouseholdAuditEntries.Add(new PantryCloud.Audit.Core.Entities.HouseholdAuditEntry
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            ActionType = Constants.Audit.ActionDeleted,
            EntityType = Constants.Audit.EntityPantryItem,
            EventId = eventId,
            OccurredAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var logger = TestHelper.MockLogger<PantryItemDeletedAuditConsumer>();
        var consumer = new PantryItemDeletedAuditConsumer(db, logger);

        var @event = new PantryItemDeletedEvent
        {
            Id = eventId,
            HouseholdId = householdId,
            ItemId = Guid.NewGuid(),
            ItemName = "Bread",
            InitiatedByUserId = Guid.NewGuid(),
            CorrelationId = Constants.Audit.CorrelationId
        };

        var context = Substitute.For<ConsumeContext<PantryItemDeletedEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var entries = await db.HouseholdAuditEntries.ToListAsync();
        entries.Count.ShouldBe(1);
    }
}
