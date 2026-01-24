using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Application.Consumers;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using Shouldly;
using Xunit;

namespace PantryCloud.Notification.UnitTests.Consumers;

public class MemberLeftHouseholdConsumerTests
{
    private readonly INotificationService _notificationService;
    private readonly MemberLeftHouseholdConsumer _consumer;

    public MemberLeftHouseholdConsumerTests()
    {
        _notificationService = Substitute.For<INotificationService>();
        var logger = Substitute.For<ILogger<MemberLeftHouseholdConsumer>>();
        _consumer = new MemberLeftHouseholdConsumer(_notificationService, logger);
    }

    [Fact]
    public async Task Consume_ShouldSendNotification_WhenEventConsumed()
    {
        var @event = new MemberLeftHouseholdEvent
        {
            HouseholdId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            MemberEmail = "test@example.com",
            LeftAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberLeftHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(context);

        await _notificationService.Received(1).SendNotificationAsync(
            Arg.Is<NotificationDto>(n => 
                n.Title == "Member Left Household" &&
                n.Type == NotificationType.Success &&
                n.CorrelationId == @event.CorrelationId &&
                n.UserId == @event.MemberId &&
                n.Message.Contains(@event.MemberEmail)), 
            Arg.Any<CancellationToken>());
    }
}
