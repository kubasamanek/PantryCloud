using MassTransit;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Application.Consumers;

public class MemberLeftHouseholdConsumer(
    INotificationService notificationService,
    ILogger<MemberLeftHouseholdConsumer> logger) 
    : ConsumerBase<MemberLeftHouseholdEvent, object>(logger)
{
    protected override async Task HandleAsync(MemberLeftHouseholdEvent @event, ConsumeContext context)
    {
        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Member Left Household",
            Message: $"Member {@event.MemberEmail} left the household.",
            Type: NotificationType.Success,
            CreatedAt: DateTime.UtcNow,
            UserId: @event.MemberId,
            CorrelationId: @event.CorrelationId);

        await notificationService.SendNotificationAsync(notification, context.CancellationToken);
        
        Logger.LogInformation(
            "Notification sent for MemberLeftHouseholdEvent - NotificationId: {NotificationId}, UserId: {UserId}",
            notification.Id,
            notification.UserId);
    }
}