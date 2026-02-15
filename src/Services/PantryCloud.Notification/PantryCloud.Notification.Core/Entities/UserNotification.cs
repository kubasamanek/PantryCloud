using PantryCloud.Notification.Core.Enums;
using PantryCloud.SharedKernel.Entities;

namespace PantryCloud.Notification.Core.Entities;

/// <summary>
/// Persisted notification for a user (for history / "what I missed" when offline).
/// </summary>
public class UserNotification : BaseEntity
{
    public Guid UserId { get; set; }
    /// <summary>Original notification Id from the DTO (for client deduplication with SignalR).</summary>
    public Guid SourceNotificationId { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}
