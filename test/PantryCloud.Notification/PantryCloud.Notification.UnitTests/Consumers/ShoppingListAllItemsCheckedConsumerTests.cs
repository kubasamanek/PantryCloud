using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Infrastructure.Consumers;
using PantryCloud.ShoppingList.Application.Events;
using PantryCloud.Notification.UnitTests;

namespace PantryCloud.Notification.UnitTests.Consumers;

public class ShoppingListAllItemsCheckedConsumerTests
{
    [Fact]
    public async Task Consume_ShouldSendToHousehold_WhenEventConsumed()
    {
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<ShoppingListAllItemsCheckedConsumer>();
        var consumer = new ShoppingListAllItemsCheckedConsumer(notificationService, logger);

        var householdId = Guid.NewGuid();
        var @event = new ShoppingListAllItemsCheckedEvent
        {
            HouseholdId = householdId,
            ShoppingListId = Guid.NewGuid(),
            ShoppingListName = Constants.TestData.ShoppingListName,
            CheckedByUserId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<ShoppingListAllItemsCheckedEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await notificationService.Received(1).SendToHouseholdAsync(
            householdId,
            Arg.Is<Core.Dtos.NotificationDto>(n =>
                n.Title == Constants.NotificationTitles.ShoppingListComplete &&
                n.Type == Core.Enums.NotificationType.Success &&
                n.Message.Contains(Constants.TestData.ShoppingListName) &&
                n.Message.Contains(Constants.TestData.CheckedOff)),
            Arg.Any<CancellationToken>());
    }
}
