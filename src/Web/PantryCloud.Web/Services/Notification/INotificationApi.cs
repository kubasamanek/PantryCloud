namespace PantryCloud.Web.Services.Notification;

/// <summary>
/// REST API for notification history (GET /me when user was offline).
/// </summary>
public interface INotificationApi
{
    /// <summary>
    /// Gets recent notifications for the current user.
    /// </summary>
    Task<IReadOnlyList<NotificationMessage>?> GetMyNotificationsAsync(int limit = 50, DateTime? since = null, CancellationToken cancellationToken = default);
}
