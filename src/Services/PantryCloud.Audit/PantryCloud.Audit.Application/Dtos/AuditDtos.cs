namespace PantryCloud.Audit.Application.Dtos;

public record AuditEntryDto(
    Guid Id,
    Guid HouseholdId,
    string ActionType,
    string EntityType,
    Guid? EntityId,
    Guid? UserId,
    string? Payload,
    DateTime OccurredAt,
    string? CorrelationId);

public record ListAuditEntriesRequestDto(
    Guid HouseholdId,
    DateTime? From,
    DateTime? To,
    string? ActionType,
    string? EntityType,
    int Page = 1,
    int PageSize = 50);

public record ListAuditEntriesResponseDto(
    IReadOnlyList<AuditEntryDto> Entries,
    int TotalCount,
    int Page,
    int PageSize);
