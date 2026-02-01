using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Infrastructure.Consumers;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.Notification.UnitTests;
using Shouldly;

namespace PantryCloud.Notification.UnitTests.Consumers;

public class PantryItemDepletedConsumerTests
{
    [Fact]
    public async Task Consume_ShouldSendToHousehold_WhenEventConsumed()
    {
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<PantryItemDepletedConsumer>();
        var consumer = new PantryItemDepletedConsumer(notificationService, logger);

        var householdId = Guid.NewGuid();
        var @event = new PantryItemDepletedEvent
        {
            HouseholdId = householdId,
            ItemId = Guid.NewGuid(),
            ItemName = "Milk",
            InitiatedByUserId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<PantryItemDepletedEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await notificationService.Received(1).SendToHouseholdAsync(
            householdId,
            Arg.Is<Core.Dtos.NotificationDto>(n =>
                n.Title == "Pantry Item Depleted" &&
                n.Type == Core.Enums.NotificationType.Info &&
                n.Message.Contains("Milk") &&
                n.Message.Contains("eaten completely")),
            Arg.Any<CancellationToken>());
    }
}
