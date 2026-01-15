using PantryCloud.Notification.Core.Enums;

namespace PantryCloud.Notification.Core.Dtos;

/// <summary>
/// Data transfer object for notifications sent to clients via SignalR.
/// </summary>
public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    NotificationType Type,
    DateTime CreatedAt,
    Guid? UserId = null,
    string? CorrelationId = null);

