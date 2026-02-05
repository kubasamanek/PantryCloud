using ErrorOr;
using PantryCloud.Audit.Application.Dtos;

namespace PantryCloud.Audit.Application;

public interface IAuditQueryService
{
    Task<ErrorOr<ListAuditEntriesResponseDto>> ListHouseholdAuditEntriesAsync(
        ListAuditEntriesRequestDto request,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);
}
