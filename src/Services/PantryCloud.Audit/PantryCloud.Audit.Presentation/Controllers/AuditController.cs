using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryCloud.Audit.Application;
using PantryCloud.Audit.Application.Dtos;
using PantryCloud.SharedKernel.Controllers;
using PantryCloud.SharedKernel.Identity;

namespace PantryCloud.Audit.Presentation.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize]
public class AuditController(IAuditQueryService auditQueryService, IUserContext userContext, IMediator mediator, IMapper mapper)
    : ApiControllerBase(mediator, mapper)
{
    [HttpGet("households/{householdId:guid}/entries")]
    [ProducesResponseType(typeof(ListAuditEntriesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListHouseholdAuditEntries(
        Guid householdId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? actionType,
        [FromQuery] string? entityType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var request = new ListAuditEntriesRequestDto(householdId, from, to, actionType, entityType, page, pageSize);
        var result = await auditQueryService.ListHouseholdAuditEntriesAsync(request, userContext.UserId, cancellationToken);
        return FromResult(result, StatusCodes.Status200OK);
    }
}
