using ErrorOr;
using MediatR;
using PantryCloud.Audit.Application;
using PantryCloud.Audit.Application.Dtos;
using PantryCloud.SharedKernel.Identity;

namespace PantryCloud.Audit.Application.Queries.Handlers;

public class ListHouseholdAuditEntriesQueryHandler(
    IAuditQueryService auditQueryService,
    IUserContext userContext) : IRequestHandler<ListHouseholdAuditEntriesQuery, ErrorOr<ListAuditEntriesResponseDto>>
{
    public async Task<ErrorOr<ListAuditEntriesResponseDto>> Handle(
        ListHouseholdAuditEntriesQuery request,
        CancellationToken cancellationToken)
    {
        return await auditQueryService.ListHouseholdAuditEntriesAsync(
            request.Request,
            userContext.UserId,
            cancellationToken);
    }
}
