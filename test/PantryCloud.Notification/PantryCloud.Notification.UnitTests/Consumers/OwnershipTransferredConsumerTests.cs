using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Infrastructure.Consumers;
using PantryCloud.Notification.Infrastructure.Persistence;
using PantryCloud.Notification.UnitTests;
using Shouldly;

namespace PantryCloud.Notification.UnitTests.Consumers;

public class OwnershipTransferredConsumerTests
{
    [Fact]
    public async Task Consume_ShouldSendToHousehold_WhenEventConsumed()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldSendToHousehold_WhenEventConsumed));
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<OwnershipTransferredConsumer>();
        var consumer = new OwnershipTransferredConsumer(db, notificationService, logger);

        var householdId = Guid.NewGuid();
        var previousOwnerId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var @event = new OwnershipTransferredEvent
        {
            HouseholdId = householdId,
            PreviousOwnerId = previousOwnerId,
            NewOwnerId = newOwnerId,
            NewOwnerEmail = "newowner@example.com",
            TransferredAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<OwnershipTransferredEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await notificationService.Received(1).SendToHouseholdAsync(
            householdId,
            Arg.Is<Core.Dtos.NotificationDto>(n =>
                n.Title == "Ownership Transferred" &&
                n.Type == Core.Enums.NotificationType.Success &&
                n.Message.Contains("newowner@example.com") &&
                n.Message.Contains("household owner")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldUseNewOwnerIdInMessage_WhenNewOwnerEmailIsNull()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldUseNewOwnerIdInMessage_WhenNewOwnerEmailIsNull));
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<OwnershipTransferredConsumer>();
        var consumer = new OwnershipTransferredConsumer(db, notificationService, logger);

        var householdId = Guid.NewGuid();
        var previousOwnerId = Guid.NewGuid();
        var newOwnerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var @event = new OwnershipTransferredEvent
        {
            HouseholdId = householdId,
            PreviousOwnerId = previousOwnerId,
            NewOwnerId = newOwnerId,
            NewOwnerEmail = null,
            TransferredAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<OwnershipTransferredEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await notificationService.Received(1).SendToHouseholdAsync(
            householdId,
            Arg.Is<Core.Dtos.NotificationDto>(n =>
                n.Title == "Ownership Transferred" &&
                n.Message.Contains(newOwnerId.ToString())),
            Arg.Any<CancellationToken>());
    }
}
