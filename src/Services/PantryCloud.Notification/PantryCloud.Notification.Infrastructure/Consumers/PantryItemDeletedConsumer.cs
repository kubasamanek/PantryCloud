using MassTransit;
using Microsoft.Extensions.Logging;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Infrastructure.Consumers;

public class PantryItemDeletedConsumer(
    INotificationService notificationService,
    ILogger<PantryItemDeletedConsumer> logger)
    : ConsumerBase<PantryItemDeletedEvent, object>(logger)
{
    protected override async Task HandleAsync(PantryItemDeletedEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received PantryItemDeletedEvent - HouseholdId: {HouseholdId}, ItemName: {ItemName}",
            @event.HouseholdId, @event.ItemName);

        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Pantry Item Removed",
            Message: $"'{@event.ItemName}' was removed from the pantry.",
            Type: NotificationType.Info,
            CreatedAt: DateTime.UtcNow,
            CorrelationId: @event.CorrelationId);

        await notificationService.SendToHouseholdAsync(@event.HouseholdId, notification, context.CancellationToken);
    }
}
