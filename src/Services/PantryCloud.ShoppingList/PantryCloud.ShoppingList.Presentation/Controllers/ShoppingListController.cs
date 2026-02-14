using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryCloud.SharedKernel.Controllers;
using PantryCloud.ShoppingList.Application.Commands;
using PantryCloud.ShoppingList.Application.Dtos;
using PantryCloud.ShoppingList.Application.Queries;

namespace PantryCloud.ShoppingList.Presentation.Controllers;

[ApiController]
[Route("api/shopping-lists")]
public class  ShoppingListController(IMediator mediator, IMapper mapper) : ApiControllerBase(mediator, mapper)
{
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CreateShoppingListResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateShoppingList([FromBody] CreateShoppingListRequestDto request, CancellationToken cancellationToken)
    {
        var command = new CreateShoppingListCommand(request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status201Created);
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ListShoppingListsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListShoppingLists(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new ListShoppingListsQuery(new ListShoppingListsRequestDto(page, pageSize));
        var result = await Mediator.Send(query, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetShoppingListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShoppingList(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetShoppingListQuery(new GetShoppingListRequestDto(id));
        var result = await Mediator.Send(query, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(DeleteShoppingListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShoppingList(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteShoppingListCommand(new DeleteShoppingListRequestDto(id));
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }
    
    [Authorize]
    [HttpPost("{id}/items")]
    [ProducesResponseType(typeof(AddShoppingListItemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddShoppingListItem(Guid id, [FromBody] AddShoppingListItemRequestDto request, CancellationToken cancellationToken)
    {
        var command = new AddShoppingListItemCommand(id, request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status201Created);
    }

    [Authorize]
    [HttpPost("{id}/items/batch")]
    [ProducesResponseType(typeof(AddShoppingListItemsBatchResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddShoppingListItemsBatch(Guid id, [FromBody] AddShoppingListItemsBatchRequestDto request, CancellationToken cancellationToken)
    {
        var command = new AddShoppingListItemsBatchCommand(id, request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status201Created);
    }

    [Authorize]
    [HttpPut("{listId}/items/{itemId}")]
    [ProducesResponseType(typeof(UpdateShoppingListItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateShoppingListItem(Guid listId, Guid itemId, [FromBody] UpdateShoppingListItemRequestDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateShoppingListItemCommand(listId, itemId, request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [Authorize]
    [HttpDelete("{listId}/items/{itemId}")]
    [ProducesResponseType(typeof(DeleteShoppingListItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShoppingListItem(Guid listId, Guid itemId, CancellationToken cancellationToken)
    {
        var command = new DeleteShoppingListItemCommand(new DeleteShoppingListItemRequestDto(listId, itemId));
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [Authorize]
    [HttpPatch("{listId}/items/{itemId}/check")]
    [ProducesResponseType(typeof(CheckShoppingListItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckShoppingListItem(Guid listId, Guid itemId, CancellationToken cancellationToken)
    {
        var command = new CheckShoppingListItemCommand(new CheckShoppingListItemRequestDto(listId, itemId));
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }
}


