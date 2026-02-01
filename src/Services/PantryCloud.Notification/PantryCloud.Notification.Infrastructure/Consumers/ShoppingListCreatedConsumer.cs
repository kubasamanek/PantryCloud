using MassTransit;
using Microsoft.Extensions.Logging;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.ShoppingList.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Infrastructure.Consumers;

public class ShoppingListCreatedConsumer(
    INotificationService notificationService,
    ILogger<ShoppingListCreatedConsumer> logger)
    : ConsumerBase<ShoppingListCreatedEvent, object>(logger)
{
    protected override async Task HandleAsync(ShoppingListCreatedEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received ShoppingListCreatedEvent - HouseholdId: {HouseholdId}, ListName: {ListName}",
            @event.HouseholdId, @event.ShoppingListName);

        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "New Shopping List",
            Message: $"'{@event.ShoppingListName}' has been created.",
            Type: NotificationType.Info,
            CreatedAt: DateTime.UtcNow,
            CorrelationId: @event.CorrelationId);

        await notificationService.SendToHouseholdExceptAsync(@event.HouseholdId, @event.CreatedByUserId, notification, context.CancellationToken);
    }
}
