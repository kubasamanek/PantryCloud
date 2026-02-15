using System.Text.Json.Serialization;

namespace PantryCloud.Web.Services.Notification;

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
