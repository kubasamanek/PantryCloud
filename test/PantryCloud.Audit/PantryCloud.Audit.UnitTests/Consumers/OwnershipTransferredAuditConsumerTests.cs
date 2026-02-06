using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PantryCloud.Audit.Infrastructure.Consumers;
using PantryCloud.Household.Application.Events;
using Shouldly;

namespace PantryCloud.Audit.UnitTests.Consumers;

public class OwnershipTransferredAuditConsumerTests
{
    [Fact]
    public async Task Consume_ShouldCreateAuditEntry_WhenEventReceived()
    {
        var householdId = Guid.NewGuid();
        var previousOwnerId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var transferredAt = DateTime.UtcNow;

        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldCreateAuditEntry_WhenEventReceived));
        var logger = TestHelper.MockLogger<OwnershipTransferredAuditConsumer>();
        var consumer = new OwnershipTransferredAuditConsumer(db, logger);

        var @event = new OwnershipTransferredEvent
        {
            Id = eventId,
            HouseholdId = householdId,
            PreviousOwnerId = previousOwnerId,
            NewOwnerId = newOwnerId,
            NewOwnerEmail = Constants.Audit.MemberEmail,
            TransferredAt = transferredAt,
            CorrelationId = Constants.Audit.CorrelationId
        };

        var context = Substitute.For<ConsumeContext<OwnershipTransferredEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var entries = await db.HouseholdAuditEntries.ToListAsync();
        entries.Count.ShouldBe(1);
        entries[0].ActionType.ShouldBe(Constants.Audit.ActionOwnershipTransferred);
        entries[0].EntityType.ShouldBe(Constants.Audit.EntityHousehold);
        entries[0].EntityId.ShouldBe(householdId);
        entries[0].UserId.ShouldBe(newOwnerId);
    }
}
