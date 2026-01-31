using MassTransit;
using Microsoft.Extensions.Logging;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Infrastructure.Consumers;

public class PantryItemDepletedConsumer(
    INotificationService notificationService,
    ILogger<PantryItemDepletedConsumer> logger)
    : ConsumerBase<PantryItemDepletedEvent, object>(logger)
{
    protected override async Task HandleAsync(PantryItemDepletedEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received PantryItemDepletedEvent - HouseholdId: {HouseholdId}, ItemName: {ItemName}",
            @event.HouseholdId, @event.ItemName);

        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Pantry Item Depleted",
            Message: $"'{@event.ItemName}' has been eaten completely and removed from the pantry.",
            Type: NotificationType.Info,
            CreatedAt: DateTime.UtcNow,
            CorrelationId: @event.CorrelationId);

        await notificationService.SendToHouseholdAsync(@event.HouseholdId, notification, context.CancellationToken);
    }
}
