using MassTransit;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;

namespace PantryCloud.Notification.Application.Consumers;

public class MemberJoinedHouseholdConsumer(
    INotificationService notificationService,
    ILogger<MemberJoinedHouseholdConsumer> logger) : IConsumer<MemberJoinedHouseholdEvent>
{
    public async Task Consume(ConsumeContext<MemberJoinedHouseholdEvent> context)
    {
        var @event = context.Message;
        
        logger.LogInformation(
            "Consumed MemberJoinedHouseholdEvent - HouseholdId: {HouseholdId}, MemberId: {MemberId}, MemberEmail: {MemberEmail}, CorrelationId: {CorrelationId}",
            @event.HouseholdId,
            @event.NewMemberId,
            @event.MemberEmail,
            @event.CorrelationId);

        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Member Joined Household",
            Message: $"Member {@event.MemberEmail ?? @event.NewMemberId.ToString()} joined the household.",
            Type: NotificationType.Success,
            CreatedAt: DateTime.UtcNow,
            UserId: @event.NewMemberId,
            CorrelationId: @event.CorrelationId);

        await notificationService.SendNotificationAsync(notification, context.CancellationToken);
        
        logger.LogInformation(
            "Notification sent for MemberJoinedHouseholdEvent - NotificationId: {NotificationId}, UserId: {UserId}",
            notification.Id,
            notification.UserId);
    }
}

