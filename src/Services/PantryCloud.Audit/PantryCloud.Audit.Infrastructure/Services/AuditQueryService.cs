using ErrorOr;
using Microsoft.EntityFrameworkCore;
using PantryCloud.Audit.Application;
using PantryCloud.Audit.Application.Dtos;
using PantryCloud.Audit.Core.Errors;
using PantryCloud.Audit.Infrastructure.Persistence;

using Microsoft.Extensions.Logging;

namespace PantryCloud.Audit.Infrastructure.Services;

public class AuditQueryService(
    AuditDbContext dbContext,
    IHouseholdMembershipRepository membershipRepository,
    ILogger<AuditQueryService> logger) : IAuditQueryService
{
    public async Task<ErrorOr<ListAuditEntriesResponseDto>> ListHouseholdAuditEntriesAsync(
        ListAuditEntriesRequestDto request,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Attempting to list audit entries for household {HouseholdId}. Requested by user {UserId}",
            request.HouseholdId, requestingUserId);

        var isMember = await membershipRepository.IsUserInHouseholdAsync(requestingUserId, request.HouseholdId, cancellationToken);
        if (!isMember)
        {
            logger.LogWarning("User {UserId} is not a member of household {HouseholdId}. Denying access.",
                requestingUserId, request.HouseholdId);
            return AuditErrors.UserNotInHousehold;
        }

        var query = dbContext.HouseholdAuditEntries
            .Where(e => e.HouseholdId == request.HouseholdId);

        if (request.From.HasValue)
            query = query.Where(e => e.OccurredAt >= request.From.Value);

        if (request.To.HasValue)
        {
            // To is interpreted as "through end of this date" (date portion only)
            // time component is ignored.
            var toExclusive = request.To.Value.Date.AddDays(1);
            query = query.Where(e => e.OccurredAt < toExclusive);
        }

        if (!string.IsNullOrWhiteSpace(request.ActionType))
            query = query.Where(e => e.ActionType == request.ActionType);

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(e => e.EntityType == request.EntityType);

        var totalCount = await query.CountAsync(cancellationToken);

        logger.LogInformation("Found {TotalCount} audit entries for household {HouseholdId}. Returning page {Page} (Size: {PageSize})",
            totalCount, request.HouseholdId, request.Page, request.PageSize);

        var entries = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new AuditEntryDto(
                e.Id,
                e.HouseholdId,
                e.ActionType,
                e.EntityType,
                e.EntityId,
                e.UserId,
                e.Payload,
                e.OccurredAt,
                e.CorrelationId))
            .ToListAsync(cancellationToken);

        return new ListAuditEntriesResponseDto(entries, totalCount, request.Page, request.PageSize);
    }
}
