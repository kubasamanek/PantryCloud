using Asp.Versioning;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PantryCloud.Recipe.Application.Commands;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Queries;
using PantryCloud.SharedKernel.Controllers;

namespace PantryCloud.Recipe.Presentation.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/recipes")]
public class RecipeController(IMediator mediator, IMapper mapper, IWebHostEnvironment environment)
    : ApiControllerBase(mediator, mapper)
{
    [Authorize]
    [HttpPost("search")]
    [ProducesResponseType(typeof(SearchRecipesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchRecipes(
        [FromBody] SearchRecipesRequestDto request,
        CancellationToken cancellationToken)
    {
        var query = new SearchRecipesQuery(request);
        var result = await Mediator.Send(query, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [Authorize]
    [HttpPost("recommend")]
    [ProducesResponseType(typeof(RecommendRecipesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecommendRecipes(
        [FromBody] RecommendRecipesRequestDto? request,
        CancellationToken cancellationToken)
    {
        var req = request ?? new RecommendRecipesRequestDto();
        var query = new RecommendRecipesQuery(req);
        var result = await Mediator.Send(query, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetRecipeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecipe(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetRecipeQuery(new GetRecipeRequestDto(id));
        var result = await Mediator.Send(query, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [HttpPost("seed")]
    [ProducesResponseType(typeof(SeedRecipesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SeedRecipes(
        [FromBody] SeedRecipesRequestDto? request = null,
        CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { error = "Seed endpoint is only available in Development environment." });
        }

        var command = new SeedRecipesCommand(request ?? new SeedRecipesRequestDto(10));
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }
}

