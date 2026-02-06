using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PantryCloud.Audit.Infrastructure.Consumers;
using PantryCloud.ShoppingList.Application.Events;
using Shouldly;

namespace PantryCloud.Audit.UnitTests.Consumers;

public class ShoppingListCreatedAuditConsumerTests
{
    [Fact]
    public async Task Consume_ShouldCreateAuditEntry_WhenEventReceived()
    {
        var householdId = Guid.NewGuid();
        var shoppingListId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldCreateAuditEntry_WhenEventReceived));
        var logger = TestHelper.MockLogger<ShoppingListCreatedAuditConsumer>();
        var consumer = new ShoppingListCreatedAuditConsumer(db, logger);

        var @event = new ShoppingListCreatedEvent
        {
            Id = eventId,
            HouseholdId = householdId,
            ShoppingListId = shoppingListId,
            ShoppingListName = Constants.Audit.ShoppingListName,
            CreatedByUserId = createdByUserId,
            OccurredAt = occurredAt,
            CorrelationId = Constants.Audit.CorrelationId
        };

        var context = Substitute.For<ConsumeContext<ShoppingListCreatedEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var entries = await db.HouseholdAuditEntries.ToListAsync();
        entries.Count.ShouldBe(1);
        entries[0].ActionType.ShouldBe(Constants.Audit.ActionCreated);
        entries[0].EntityType.ShouldBe(Constants.Audit.EntityShoppingList);
        entries[0].EntityId.ShouldBe(shoppingListId);
        entries[0].UserId.ShouldBe(createdByUserId);
        entries[0].Payload!.ShouldContain(Constants.Audit.ShoppingListName);
    }
}
