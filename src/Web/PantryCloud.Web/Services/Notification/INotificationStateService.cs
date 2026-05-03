namespace PantryCloud.Web.Services.Notification;

/// <summary>
/// In-memory state for notifications (from SignalR and history). Read/dismiss is client-only.
/// </summary>
public interface INotificationStateService
{
    /// <summary>
    /// Raised when the list or unread count changes (subscribe to refresh UI).
    /// </summary>
    event Action? StateChanged;

    int UnreadCount { get; }

    /// <summary>
    /// Notifications for the panel (unread first, then by CreatedAt descending).
    /// </summary>
    IReadOnlyList<NotificationMessage> GetListForPanel();

    void Add(NotificationMessage message);
    void AddRange(IEnumerable<NotificationMessage> messages);
    void MarkAsRead(Guid id);
    void MarkAllAsRead();
    bool IsRead(Guid id);
}
