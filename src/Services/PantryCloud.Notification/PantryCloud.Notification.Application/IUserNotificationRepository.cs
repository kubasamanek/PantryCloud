using PantryCloud.Notification.Core.Dtos;

namespace PantryCloud.Notification.Application;

/// <summary>
/// Repository for persisting and querying user notifications (history).
/// </summary>
public interface IUserNotificationRepository
{
    /// <summary>
    /// Adds a notification for a user (persist for history).
    /// </summary>
    Task AddAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds the same notification for multiple users.
    /// </summary>
    Task AddForUsersAsync(IReadOnlyList<Guid> userIds, NotificationDto notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent notifications for a user, ordered by CreatedAt descending.
    /// </summary>
    Task<IReadOnlyList<NotificationDto>> GetByUserIdAsync(Guid userId, int limit = 50, DateTime? since = null, CancellationToken cancellationToken = default);
}
