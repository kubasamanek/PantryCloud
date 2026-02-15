using System.Text.Json.Serialization;

namespace PantryCloud.Web.Services.Sessions;

public record SessionDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("lastUsedAt")] DateTime? LastUsedAt,
    [property: JsonPropertyName("deviceName")] string? DeviceName,
    [property: JsonPropertyName("isCurrent")] bool IsCurrent);

public record ListSessionsResponse(
    [property: JsonPropertyName("sessions")] List<SessionDto> Sessions);
