using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Infrastructure.Consumers;
using PantryCloud.ShoppingList.Application.Events;
using PantryCloud.Notification.UnitTests;

namespace PantryCloud.Notification.UnitTests.Consumers;

public class ShoppingListCreatedConsumerTests
{
    [Fact]
    public async Task Consume_ShouldSendToHouseholdExceptCreator_WhenEventConsumed()
    {
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<ShoppingListCreatedConsumer>();
        var consumer = new ShoppingListCreatedConsumer(notificationService, logger);

        var householdId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();
        var @event = new ShoppingListCreatedEvent
        {
            HouseholdId = householdId,
            ShoppingListId = Guid.NewGuid(),
            ShoppingListName = Constants.TestData.ShoppingListName,
            CreatedByUserId = createdByUserId,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<ShoppingListCreatedEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await notificationService.Received(1).SendToHouseholdExceptAsync(
            householdId,
            createdByUserId,
            Arg.Is<Core.Dtos.NotificationDto>(n =>
                n.Title == Constants.NotificationTitles.NewShoppingList &&
                n.Type == Core.Enums.NotificationType.Info &&
                n.Message.Contains(Constants.TestData.ShoppingListName) &&
                n.Message.Contains(Constants.TestData.Created)),
            Arg.Any<CancellationToken>());
    }
}
