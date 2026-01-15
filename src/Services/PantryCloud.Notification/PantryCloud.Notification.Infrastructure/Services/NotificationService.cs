using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Infrastructure.Hubs;

namespace PantryCloud.Notification.Infrastructure.Services;

public class NotificationService(
    IHubContext<NotificationHub> hubContext,
    ILogger<NotificationService> logger) : INotificationService
{
    private const string NotificationMethod = "ReceiveNotification";
    private const string NotificationGroupPrefix = "user_";

    public Task SendNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Sending notification - Id: {NotificationId}, Title: {Title}, Type: {Type}, UserId: {UserId}, CorrelationId: {CorrelationId}",
            notification.Id,
            notification.Title,
            notification.Type,
            notification.UserId,
            notification.CorrelationId);
        
        if (notification.UserId.HasValue)
        {
            var groupName = $"{NotificationGroupPrefix}{notification.UserId.Value}";
            
            logger.LogInformation(
                "Would send notification to SignalR group {GroupName} - Notification: {Notification}",
                groupName,
                notification);

            // TODO: implement once hub consumer is implemented
            // await hubContext.Clients.Group(groupName).SendAsync(NotificationMethod, notification, cancellationToken);
        }
        else
        {
            logger.LogWarning("Notification {NotificationId} has no UserId, cannot send to specific user", notification.Id);
        }

        return Task.CompletedTask;
    }

    public Task BroadcastNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Broadcasting notification to all clients - Id: {NotificationId}, Title: {Title}, Type: {Type}, CorrelationId: {CorrelationId}",
            notification.Id,
            notification.Title,
            notification.Type,
            notification.CorrelationId);

        logger.LogInformation(
            "Would broadcast notification to all SignalR clients - Notification: {Notification}",
            notification);
        return Task.CompletedTask;

        // TODO: implement once hub consumer is implemented
        // await hubContext.Clients.All.SendAsync(NotificationMethod, notification, cancellationToken);
    }
}

