using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using PantryCloud.Recipe.Application.Commands;
using PantryCloud.Recipe.Application.Data;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;

namespace PantryCloud.Recipe.Application.Commands.Handlers;

public class SeedRecipesCommandHandler(IRecipeRepository repository, ILogger<SeedRecipesCommandHandler> logger)
    : IRequestHandler<SeedRecipesCommand, ErrorOr<SeedRecipesResponseDto>>
{
    public async Task<ErrorOr<SeedRecipesResponseDto>> Handle(
        SeedRecipesCommand request,
        CancellationToken cancellationToken)
    {
        var count = request.Request.Count;
        logger.LogInformation("Seed recipes requested: count={Count}", count);

        var recipes = DefaultRecipeData.GetDefaultRecipes().Take(count).ToList();
        var result = await repository.SeedRecipesAsync(recipes, cancellationToken);

        if (result.IsError)
        {
            logger.LogWarning("Seed recipes failed: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
            return result.Errors;
        }

        logger.LogInformation("Seed recipes completed: {RecipesCreated} recipes created", result.Value.RecipesCreated);
        return result;
    }
}
