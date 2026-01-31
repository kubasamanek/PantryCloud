using MassTransit;
using Microsoft.Extensions.Logging;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.ShoppingList.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Infrastructure.Consumers;

public class ShoppingListAllItemsCheckedConsumer(
    INotificationService notificationService,
    ILogger<ShoppingListAllItemsCheckedConsumer> logger)
    : ConsumerBase<ShoppingListAllItemsCheckedEvent, object>(logger)
{
    protected override async Task HandleAsync(ShoppingListAllItemsCheckedEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received ShoppingListAllItemsCheckedEvent - HouseholdId: {HouseholdId}, ListName: {ListName}",
            @event.HouseholdId, @event.ShoppingListName);

        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Shopping List Complete",
            Message: $"All items in '{@event.ShoppingListName}' have been checked off!",
            Type: NotificationType.Success,
            CreatedAt: DateTime.UtcNow,
            CorrelationId: @event.CorrelationId);

        await notificationService.SendToHouseholdAsync(@event.HouseholdId, notification, context.CancellationToken);
    }
}
