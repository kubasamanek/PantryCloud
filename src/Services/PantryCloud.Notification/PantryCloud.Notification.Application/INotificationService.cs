using PantryCloud.Notification.Core.Dtos;

namespace PantryCloud.Notification.Application;

/// <summary>
/// Service interface for sending notifications to clients via SignalR.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to a specific user.
    /// </summary>
    Task SendToUserAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to all members of a household.
    /// </summary>
    Task SendToHouseholdAsync(Guid householdId, NotificationDto notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to all household members except the specified user.
    /// </summary>
    Task SendToHouseholdExceptAsync(Guid householdId, Guid excludeUserId, NotificationDto notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to all connected clients (use sparingly).
    /// </summary>
    Task BroadcastNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default);
}

