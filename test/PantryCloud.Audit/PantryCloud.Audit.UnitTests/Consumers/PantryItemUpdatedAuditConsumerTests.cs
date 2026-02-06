using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PantryCloud.Audit.Infrastructure.Consumers;
using PantryCloud.Pantry.Application.Events;
using Shouldly;

namespace PantryCloud.Audit.UnitTests.Consumers;

public class PantryItemUpdatedAuditConsumerTests
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
        var logger = TestHelper.MockLogger<PantryItemUpdatedAuditConsumer>();
        var consumer = new PantryItemUpdatedAuditConsumer(db, logger);

        var @event = new PantryItemUpdatedEvent
        {
            Id = eventId,
            HouseholdId = householdId,
            ItemId = itemId,
            ItemName = Constants.Audit.ItemName,
            Quantity = Constants.Audit.Quantity,
            UserId = userId,
            OccurredAt = occurredAt,
            CorrelationId = Constants.Audit.CorrelationId
        };

        var context = Substitute.For<ConsumeContext<PantryItemUpdatedEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var entries = await db.HouseholdAuditEntries.ToListAsync();
        entries.Count.ShouldBe(1);
        entries[0].ActionType.ShouldBe(Constants.Audit.ActionUpdated);
        entries[0].EntityType.ShouldBe(Constants.Audit.EntityPantryItem);
        entries[0].EntityId.ShouldBe(itemId);
        entries[0].UserId.ShouldBe(userId);
        entries[0].Payload!.ShouldContain(Constants.Audit.ItemName);
    }
}
