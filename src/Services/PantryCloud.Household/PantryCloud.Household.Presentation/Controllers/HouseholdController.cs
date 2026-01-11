using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PantryCloud.Household.Application.Commands;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Application.Queries;
using PantryCloud.SharedKernel.Controllers;

namespace PantryCloud.Household.Presentation.Controllers;

[ApiController]
[Route("api/households")]
public class HouseholdController(IMediator mediator, IMapper mapper) : ApiControllerBase(mediator, mapper)
{
    [HttpPost("")]
    [ProducesResponseType(typeof(CreateHouseholdResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateHousehold([FromBody] CreateHouseholdRequestDto request, CancellationToken cancellationToken)
    {
        var command = Mapper.Map<CreateHouseholdCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status201Created);
    }
    
    [HttpGet("me")]
    [ProducesResponseType(typeof(GetCurrentHouseholdResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentHousehold(CancellationToken cancellationToken)
    {
        var query = new GetCurrentHouseholdQuery(new GetCurrentHouseholdRequestDto());
        var result = await Mediator.Send(query, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [HttpPost("invite")]
    [ProducesResponseType(typeof(SendHouseholdInvitationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendHouseholdInvitation(SendHouseholdInvitationRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<SendHouseholdInvitationCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);
        
        return FromResult(result, StatusCodes.Status200OK);
    }
    
    [HttpPost("join")]
    [ProducesResponseType(typeof(AcceptHouseholdInvitationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AcceptHouseholdInvitation(AcceptHouseholdInvitationRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<AcceptHouseholdInvitationCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);
        
        return FromResult(result, StatusCodes.Status200OK);
    }
    
    [HttpPost("leave")]
    [ProducesResponseType(typeof(LeaveHouseholdResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LeaveHousehold(LeaveHouseholdRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<LeaveHouseholdCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);
        
        return FromResult(result, StatusCodes.Status200OK);
    }

}