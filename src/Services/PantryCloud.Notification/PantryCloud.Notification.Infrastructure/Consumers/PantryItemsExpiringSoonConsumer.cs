using MassTransit;
using Microsoft.Extensions.Logging;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Infrastructure.Consumers;

public class PantryItemsExpiringSoonConsumer(
    INotificationService notificationService,
    ILogger<PantryItemsExpiringSoonConsumer> logger)
    : ConsumerBase<PantryItemsExpiringSoonEvent, object>(logger)
{
    protected override async Task HandleAsync(PantryItemsExpiringSoonEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received PantryItemsExpiringSoonEvent - HouseholdId: {HouseholdId}, ItemCount: {ItemCount}",
            @event.HouseholdId, @event.Items.Count);

        if (@event.Items.Count == 0)
            return;

        var messageParts = @event.Items
            .Select(item => item.DaysUntilExpiry switch
            {
                0 => $"'{item.Name}' expires today",
                1 => $"'{item.Name}' will expire in one day",
                2 => $"'{item.Name}' will expire in two days",
                _ => $"'{item.Name}' expires soon"
            })
            .ToList();

        var message = "Items expiring soon: " + string.Join(". ", messageParts);

        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Pantry Items Expiring Soon",
            Message: message,
            Type: NotificationType.Warning,
            CreatedAt: DateTime.UtcNow,
            CorrelationId: @event.CorrelationId);

        await notificationService.SendToHouseholdAsync(@event.HouseholdId, notification, context.CancellationToken);
    }
}
