using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Notification.Application;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Infrastructure.Consumers;
using PantryCloud.Notification.UnitTests;

namespace PantryCloud.Notification.UnitTests.Consumers;

public class PreferenceAddedConsumerTests
{
    [Fact]
    public async Task Consume_ShouldSendToHouseholdExceptUser_WhenEventConsumed()
    {
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<PreferenceAddedConsumer>();
        var consumer = new PreferenceAddedConsumer(notificationService, logger);

        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var @event = new PreferenceAddedEvent
        {
            HouseholdId = householdId,
            UserId = userId,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<PreferenceAddedEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await notificationService.Received(1).SendToHouseholdExceptAsync(
            householdId,
            userId,
            Arg.Is<Core.Dtos.NotificationDto>(n =>
                n.Title == Constants.NotificationTitles.PreferencesUpdated &&
                n.Type == Core.Enums.NotificationType.Info &&
                n.Message.Contains(Constants.TestData.DietaryPreferences)),
            Arg.Any<CancellationToken>());
    }
}
