using MassTransit;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;

namespace PantryCloud.Notification.Application.Consumers;

public class MemberLeftHouseholdConsumer(
    INotificationService notificationService,
    ILogger<MemberLeftHouseholdConsumer> logger) : IConsumer<MemberLeftHouseholdEvent>
{
    public async Task Consume(ConsumeContext<MemberLeftHouseholdEvent> context)
    {
        var @event = context.Message;
        
        logger.LogInformation(
            "Consumed MemberLeftHouseholdEvent - HouseholdId: {HouseholdId}, MemberEmail: {MemberEmail}, CorrelationId: {CorrelationId}",
            @event.HouseholdId,
            @event.MemberEmail,
            @event.CorrelationId);

        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Member Left Household",
            Message: $"Member {@event.MemberEmail} left the household.",
            Type: NotificationType.Success,
            CreatedAt: DateTime.UtcNow,
            CorrelationId: @event.CorrelationId);

        await notificationService.SendNotificationAsync(notification, context.CancellationToken);
        
        logger.LogInformation(
            "Notification sent for MemberLeftHouseholdEvent - NotificationId: {NotificationId}, UserId: {UserId}",
            notification.Id,
            notification.UserId);
    }
}