using Asp.Versioning;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryCloud.Identity.Application.Commands;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.SharedKernel.Controllers;

namespace PantryCloud.Identity.Presentation.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/auth")]
[Authorize]
public class SessionsController(IMediator mediator, IMapper mapper) : ApiControllerBase(mediator, mapper)
{
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(ListSessionsResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListSessions(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ListSessionsCommand(), cancellationToken);
        return FromResult(result, StatusCodes.Status200OK);
    }

    [HttpDelete("sessions/{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RevokeSessionCommand(sessionId), cancellationToken);
        return FromResult(result, StatusCodes.Status204NoContent);
    }

    [HttpDelete("sessions/others")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAllOtherSessions(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RevokeAllOtherSessionsCommand(), cancellationToken);
        return FromResult(result, StatusCodes.Status204NoContent);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new LogoutCommand(request), cancellationToken);
        return FromResult(result, StatusCodes.Status204NoContent);
    }
}
