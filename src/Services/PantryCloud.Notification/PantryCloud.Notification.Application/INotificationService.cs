using PantryCloud.Notification.Core.Dtos;

namespace PantryCloud.Notification.Application;

/// <summary>
/// Service interface for sending notifications to clients.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to a specific user.
    /// </summary>
    /// <param name="notification">The notification to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends a notification to all connected clients.
    /// </summary>
    /// <param name="notification">The notification to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task BroadcastNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default);
}

