using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Application.Consumers;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;

namespace PantryCloud.Notification.UnitTests.Consumers;

public class MemberJoinedHouseholdConsumerTests
{
    private readonly INotificationService _notificationService;
    private readonly MemberJoinedHouseholdConsumer _consumer;

    public MemberJoinedHouseholdConsumerTests()
    {
        _notificationService = Substitute.For<INotificationService>();
        var logger = Substitute.For<ILogger<MemberJoinedHouseholdConsumer>>();
        _consumer = new MemberJoinedHouseholdConsumer(_notificationService, logger);
    }

    [Fact]
    public async Task Consume_ShouldSendNotification_WhenEventConsumed()
    {
        var @event = new MemberJoinedHouseholdEvent
        {
            HouseholdId = Guid.NewGuid(),
            NewMemberId = Guid.NewGuid(),
            MemberEmail = "test@example.com",
            JoinedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberJoinedHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(context);

        await _notificationService.Received(1).SendNotificationAsync(
            Arg.Is<NotificationDto>(n => 
                n.Title == "Member Joined Household" &&
                n.UserId == @event.NewMemberId &&
                n.Type == NotificationType.Success &&
                n.CorrelationId == @event.CorrelationId &&
                n.Message.Contains(@event.MemberEmail)), 
            Arg.Any<CancellationToken>());
    }
}
