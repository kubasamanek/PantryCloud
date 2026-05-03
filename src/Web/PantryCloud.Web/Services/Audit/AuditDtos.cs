using System.Text.Json.Serialization;

namespace PantryCloud.Web.Services.Audit;

public record AuditEntryDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("householdId")] Guid HouseholdId,
    [property: JsonPropertyName("actionType")] string ActionType,
    [property: JsonPropertyName("entityType")] string EntityType,
    [property: JsonPropertyName("entityId")] Guid? EntityId,
    [property: JsonPropertyName("userId")] Guid? UserId,
    [property: JsonPropertyName("payload")] string? Payload,
    [property: JsonPropertyName("occurredAt")] DateTime OccurredAt,
    [property: JsonPropertyName("correlationId")] string? CorrelationId);

public record ListAuditEntriesResponseDto(
    [property: JsonPropertyName("entries")] IReadOnlyList<AuditEntryDto> Entries,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize);
