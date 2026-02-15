using PantryCloud.Notification.Core.Enums;
using PantryCloud.SharedKernel.Entities;

namespace PantryCloud.Notification.Core.Entities;

/// <summary>
/// Persisted notification for a user (for history / "what I missed" when offline).
/// </summary>
public class UserNotification : BaseEntity
{
    public Guid UserId { get; init; }
    public Guid SourceNotificationId { get; init; }
    public string Title { get; init; } = "";
    public string Message { get; init; } = "";
    public NotificationType Type { get; init; }
    public DateTime CreatedAt { get; init; }
}
