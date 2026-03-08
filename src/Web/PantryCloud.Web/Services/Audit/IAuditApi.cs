namespace PantryCloud.Web.Services.Audit;

public interface IAuditApi
{
    Task<ListAuditEntriesResponseDto?> ListHouseholdAuditEntriesAsync(
        Guid householdId,
        DateTime? from = null,
        DateTime? to = null,
        string? actionType = null,
        string? entityType = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
