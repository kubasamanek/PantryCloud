namespace PantryCloud.Web.Services.Notification;

public interface INotificationApi
{
    Task<IReadOnlyList<NotificationMessage>?> GetMyNotificationsAsync(int limit = 50, DateTime? since = null, CancellationToken cancellationToken = default);
}
