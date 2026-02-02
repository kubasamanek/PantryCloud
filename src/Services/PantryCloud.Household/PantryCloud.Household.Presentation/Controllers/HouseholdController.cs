using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryCloud.Household.Application.Commands;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Application.Queries;
using PantryCloud.SharedKernel.Controllers;

namespace PantryCloud.Household.Presentation.Controllers;

[ApiController]
[Route("api/households")]
[Authorize]
public class HouseholdController(IMediator mediator, IMapper mapper) : ApiControllerBase(mediator, mapper)
{
    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(typeof(CreateHouseholdResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateHousehold([FromBody] CreateHouseholdRequestDto request, CancellationToken cancellationToken)
    {
        var command = Mapper.Map<CreateHouseholdCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status201Created);
    }
    
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(GetCurrentHouseholdResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentHousehold(CancellationToken cancellationToken)
    {
        var query = new GetCurrentHouseholdQuery(new GetCurrentHouseholdRequestDto());
        var result = await Mediator.Send(query, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [Authorize]
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
    
    [Authorize]
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

    [HttpGet("me/preferences")]
    [ProducesResponseType(typeof(GetMyPreferencesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyPreferences(CancellationToken cancellationToken)
    {
        var query = new GetMyPreferencesQuery(new GetMyPreferencesRequestDto());
        var result = await Mediator.Send(query, cancellationToken);
        return FromResult(result, StatusCodes.Status200OK);
    }

    [HttpPut("me/preferences")]
    [ProducesResponseType(typeof(UpdateMyPreferencesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyPreferences([FromBody] UpdateMyPreferencesRequestDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateMyPreferencesCommand(request);
        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result, StatusCodes.Status200OK);
    }

    [HttpGet("me/profile")]
    [ProducesResponseType(typeof(GetMyProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var query = new GetMyProfileQuery(new GetMyProfileRequestDto());
        var result = await Mediator.Send(query, cancellationToken);
        return FromResult(result, StatusCodes.Status200OK);
    }

    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(UpdateMyProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileRequestDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateMyProfileCommand(request);
        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result, StatusCodes.Status200OK);
    }

    [HttpGet("me/members/preferences")]
    [ProducesResponseType(typeof(GetHouseholdMembersPreferencesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHouseholdMembersPreferences(CancellationToken cancellationToken)
    {
        var query = new GetHouseholdMembersPreferencesQuery(new GetHouseholdMembersPreferencesRequestDto());
        var result = await Mediator.Send(query, cancellationToken);
        return FromResult(result, StatusCodes.Status200OK);
    }
}