using ErrorOr;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Core.Errors;
using RecipeEntity = PantryCloud.Recipe.Core.Entities.Recipe;

namespace PantryCloud.Recipe.Infrastructure.Persistence;

public class MongoRecipeRepository(RecipeDbContext dbContext, ILogger<MongoRecipeRepository> logger) : IRecipeRepository
{
    public async Task<ErrorOr<RecipeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting recipe {RecipeId}", id);

        var recipe = await dbContext.Recipes
            .Find(r => r.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (recipe == null)
        {
            logger.LogWarning("Recipe {RecipeId} not found", id);
            return RecipeErrors.RecipeNotFound;
        }

        logger.LogInformation("Retrieved recipe {RecipeId} ({Title})", id, recipe.Title);
        return MapToDto(recipe);
    }

    public async Task<ErrorOr<SeedRecipesResponseDto>> SeedRecipesAsync(
        List<RecipeEntity> recipes,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Seeding {Count} recipes into MongoDB", recipes.Count);

        await dbContext.Recipes.InsertManyAsync(recipes, cancellationToken: cancellationToken);

        logger.LogInformation("Seeded {Count} recipes successfully", recipes.Count);
        return new SeedRecipesResponseDto(recipes.Count);
    }

    private static RecipeDto MapToDto(RecipeEntity recipe)
    {
        return new RecipeDto(
            Id: recipe.Id,
            Title: recipe.Title,
            Description: recipe.Description,
            Ingredients: recipe.Ingredients.Select(i => new IngredientDto(
                Name: i.Name,
                Quantity: i.Quantity,
                Unit: i.Unit,
                IsOptional: i.IsOptional
            )).ToList(),
            Steps: recipe.Steps.Select(s => new RecipeStepDto(
                StepNumber: s.StepNumber,
                Instruction: s.Instruction,
                DurationMinutes: s.DurationMinutes,
                ImageUrl: s.ImageUrl
            )).ToList(),
            Tags: recipe.Tags,
            DietaryLabels: recipe.DietaryLabels,
            Source: recipe.Source,
            PrepTimeMinutes: recipe.PrepTimeMinutes,
            CookTimeMinutes: recipe.CookTimeMinutes,
            Servings: recipe.Servings,
            ImageUrl: recipe.ImageUrl,
            CreatedAt: recipe.CreatedAt,
            UpdatedAt: recipe.UpdatedAt
        );
    }
}

