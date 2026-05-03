using MassTransit;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Infrastructure.Consumers;

public class PreferenceAddedConsumer(
    INotificationService notificationService,
    ILogger<PreferenceAddedConsumer> logger)
    : ConsumerBase<PreferenceAddedEvent, object>(logger)
{
    protected override async Task HandleAsync(PreferenceAddedEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received PreferenceAddedEvent - HouseholdId: {HouseholdId}, UserId: {UserId}",
            @event.HouseholdId, @event.UserId);

        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Preferences Updated",
            Message: "A household member has updated their dietary preferences.",
            Type: NotificationType.Info,
            CreatedAt: DateTime.UtcNow,
            CorrelationId: @event.CorrelationId);

        await notificationService.SendToHouseholdExceptAsync(@event.HouseholdId, @event.UserId, notification, context.CancellationToken);
    }
}
