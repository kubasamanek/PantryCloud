using MassTransit;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Application.Consumers;

public class MemberJoinedHouseholdConsumer(
    INotificationService notificationService,
    ILogger<MemberJoinedHouseholdConsumer> logger) 
    : ConsumerBase<MemberJoinedHouseholdEvent, object>(logger)
{
    protected override async Task HandleAsync(MemberJoinedHouseholdEvent @event, ConsumeContext context)
    {
        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Member Joined Household",
            Message: $"Member {@event.MemberEmail ?? @event.NewMemberId.ToString()} joined the household.",
            Type: NotificationType.Success,
            CreatedAt: DateTime.UtcNow,
            UserId: @event.NewMemberId,
            CorrelationId: @event.CorrelationId);

        await notificationService.SendNotificationAsync(notification, context.CancellationToken);
        
        Logger.LogInformation(
            "Notification sent for MemberJoinedHouseholdEvent - NotificationId: {NotificationId}, UserId: {UserId}",
            notification.Id,
            notification.UserId);
    }
}

