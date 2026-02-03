using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Infrastructure.Consumers;
using PantryCloud.Notification.UnitTests;
using PantryCloud.Pantry.Application.Events;
using Shouldly;

namespace PantryCloud.Notification.UnitTests.Consumers;

public class PantryItemsExpiringSoonConsumerTests
{
    [Fact]
    public async Task Consume_ShouldSendToHousehold_WithFormattedMessage_WhenItemsExpireAtDifferentTimes()
    {
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<PantryItemsExpiringSoonConsumer>();
        var consumer = new PantryItemsExpiringSoonConsumer(notificationService, logger);

        var householdId = Guid.NewGuid();
        var @event = new PantryItemsExpiringSoonEvent
        {
            HouseholdId = householdId,
            Items =
            [
                new ExpiringItemDto(Guid.NewGuid(), "Milk", DateTime.UtcNow, 0),
                new ExpiringItemDto(Guid.NewGuid(), "Bread", DateTime.UtcNow.AddDays(1), 1),
                new ExpiringItemDto(Guid.NewGuid(), "Eggs", DateTime.UtcNow.AddDays(2), 2)
            ],
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<PantryItemsExpiringSoonEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await notificationService.Received(1).SendToHouseholdAsync(
            householdId,
            Arg.Is<Core.Dtos.NotificationDto>(n =>
                n.Title == "Pantry Items Expiring Soon" &&
                n.Type == Core.Enums.NotificationType.Warning &&
                n.Message.Contains("'Milk' expires today") &&
                n.Message.Contains("'Bread' will expire in one day") &&
                n.Message.Contains("'Eggs' will expire in two days") &&
                n.Message.Contains("Items expiring soon")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldNotSend_WhenItemsListIsEmpty()
    {
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<PantryItemsExpiringSoonConsumer>();
        var consumer = new PantryItemsExpiringSoonConsumer(notificationService, logger);

        var householdId = Guid.NewGuid();
        var @event = new PantryItemsExpiringSoonEvent
        {
            HouseholdId = householdId,
            Items = [],
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<PantryItemsExpiringSoonEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await notificationService.DidNotReceive().SendToHouseholdAsync(
            Arg.Any<Guid>(),
            Arg.Any<Core.Dtos.NotificationDto>(),
            Arg.Any<CancellationToken>());
    }
}
