using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Audit.Infrastructure.Consumers;
using PantryCloud.Pantry.Application.Events;
using Shouldly;

namespace PantryCloud.Audit.UnitTests.Consumers;

public class PantryItemCreatedAuditConsumerTests
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
        var logger = TestHelper.MockLogger<PantryItemCreatedAuditConsumer>();
        var consumer = new PantryItemCreatedAuditConsumer(db, logger);

        var @event = new PantryItemCreatedEvent
        {
            Id = eventId,
            HouseholdId = householdId,
            ItemId = itemId,
            ItemName = "Milk",
            Quantity = 2.5m,
            UserId = userId,
            OccurredAt = occurredAt,
            CorrelationId = "test-correlation"
        };

        var context = Substitute.For<ConsumeContext<PantryItemCreatedEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var entries = await db.HouseholdAuditEntries.ToListAsync();
        entries.Count.ShouldBe(1);
        entries[0].HouseholdId.ShouldBe(householdId);
        entries[0].ActionType.ShouldBe("Created");
        entries[0].EntityType.ShouldBe("PantryItem");
        entries[0].EntityId.ShouldBe(itemId);
        entries[0].UserId.ShouldBe(userId);
        entries[0].EventId.ShouldBe(eventId);
        entries[0].Payload!.ShouldContain("Milk");
        entries[0].Payload!.ShouldContain("2.5");
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
            ActionType = "Created",
            EntityType = "PantryItem",
            EventId = eventId,
            OccurredAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var logger = TestHelper.MockLogger<PantryItemCreatedAuditConsumer>();
        var consumer = new PantryItemCreatedAuditConsumer(db, logger);

        var @event = new PantryItemCreatedEvent
        {
            Id = eventId,
            HouseholdId = householdId,
            ItemId = Guid.NewGuid(),
            ItemName = "Bread",
            Quantity = 1,
            UserId = Guid.NewGuid(),
            CorrelationId = "test-correlation"
        };

        var context = Substitute.For<ConsumeContext<PantryItemCreatedEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var entries = await db.HouseholdAuditEntries.ToListAsync();
        entries.Count.ShouldBe(1);
        entries[0].EntityId.ShouldNotBe(@event.ItemId);
    }
}
