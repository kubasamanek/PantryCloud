using System.Text.Json.Serialization;

namespace PantryCloud.Web.Services.Notification;

/// <summary>
/// Client-side model for a notification (matches backend NotificationDto, camelCase).
/// Type: 0=Info, 1=Success, 2=Warning, 3=Error.
/// </summary>
public record NotificationMessage
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("title")] public string Title { get; init; } = "";
    [JsonPropertyName("message")] public string Message { get; init; } = "";
    [JsonPropertyName("type")] public int Type { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
    [JsonPropertyName("userId")] public Guid? UserId { get; init; }
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; init; }
}
