using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryCloud.Pantry.Application.Commands;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.Application.Queries;
using PantryCloud.SharedKernel.Controllers;

namespace PantryCloud.Pantry.Presentation.Controllers;

[ApiController]
[Route("api/pantry")]
public class PantryController(IMediator mediator, IMapper mapper) : ApiControllerBase(mediator, mapper)
{
    [Authorize]
    [HttpPost("items")]
    [ProducesResponseType(typeof(CreatePantryItemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePantryItem([FromBody] CreatePantryItemRequestDto request, CancellationToken cancellationToken)
    {
        var command = Mapper.Map<CreatePantryItemCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status201Created);
    }

    [Authorize]
    [HttpPut("items/{id}")]
    [ProducesResponseType(typeof(UpdatePantryItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdatePantryItem(Guid id, [FromBody] UpdatePantryItemRequestDto request, CancellationToken cancellationToken)
    {
        var command = new UpdatePantryItemCommand(id, request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [Authorize]
    [HttpDelete("items/{id}")]
    [ProducesResponseType(typeof(DeletePantryItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePantryItem(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePantryItemCommand(new DeletePantryItemRequestDto(id));
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [Authorize]
    [HttpGet("items/{id}")]
    [ProducesResponseType(typeof(GetPantryItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPantryItem(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPantryItemQuery(new GetPantryItemRequestDto(id));
        var result = await Mediator.Send(query, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [Authorize]
    [HttpGet("items")]
    [ProducesResponseType(typeof(ListPantryItemsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListPantryItems(
        [FromQuery] string? category,
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new ListPantryItemsQuery(new ListPantryItemsRequestDto(category, searchTerm, page, pageSize));
        var result = await Mediator.Send(query, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }
}

