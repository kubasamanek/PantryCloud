using ErrorOr;
using MongoDB.Driver;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Core.Errors;
using RecipeEntity = PantryCloud.Recipe.Core.Entities.Recipe;

namespace PantryCloud.Recipe.Infrastructure.Persistence;

public class MongoRecipeRepository(RecipeDbContext dbContext) : IRecipeRepository
{
    public async Task<ErrorOr<RecipeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recipe = await dbContext.Recipes
            .Find(r => r.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (recipe == null)
        {
            return RecipeErrors.RecipeNotFound;
        }

        return MapToDto(recipe);
    }

    public async Task<ErrorOr<SeedRecipesResponseDto>> SeedRecipesAsync(
        List<RecipeEntity> recipes,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Recipes.InsertManyAsync(recipes, cancellationToken: cancellationToken);
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

