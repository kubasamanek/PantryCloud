using ErrorOr;
using MongoDB.Bson;
using MongoDB.Driver;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Core.Errors;
using PantryCloud.Recipe.Infrastructure.Persistence;
using RecipeEntity = PantryCloud.Recipe.Core.Entities.Recipe;
using IngredientEntity = PantryCloud.Recipe.Core.Entities.Ingredient;

namespace PantryCloud.Recipe.Infrastructure.Services;

public class LocalMongoRecipeSearchService(RecipeDbContext dbContext) : IRecipeSearchService
{
    public async Task<ErrorOr<SearchRecipesResponseDto>> SearchRecipesAsync(
        SearchRecipesRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Ingredients.Count == 0)
        {
            return RecipeErrors.InvalidSearchRequest;
        }

        // Normalize ingredient names for case-insensitive search
        var normalizedIngredients = request.Ingredients
            .Select(i => i.Trim().ToLowerInvariant())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .ToList();

        if (normalizedIngredients.Count == 0)
        {
            return RecipeErrors.InvalidSearchRequest;
        }

        // Build MongoDB filter: Find recipes where ANY ingredient name matches ANY search term
        // MongoDB doesn't support ToLower() in filter, so we use case-insensitive regex
        var filterBuilder = Builders<RecipeEntity>.Filter;
        var filters = normalizedIngredients.Select(ingredient =>
            filterBuilder.ElemMatch(
                r => r.Ingredients,
                Builders<IngredientEntity>.Filter.Regex(i => i.Name, new BsonRegularExpression(ingredient, "i"))
            )
        );

        var filter = filterBuilder.Or(filters);

        // Get all matching recipes
        var recipes = await dbContext.Recipes
            .Find(filter)
            .ToListAsync(cancellationToken);

        // Calculate match scores (number of matching ingredients)
        var recipesWithScores = recipes.Select(recipe =>
        {
            var matchCount = recipe.Ingredients.Count(ingredient =>
                normalizedIngredients.Contains(ingredient.Name.ToLowerInvariant())
            );
            return new { Recipe = recipe, MatchCount = matchCount };
        })
        .OrderByDescending(x => x.MatchCount)
        .ThenBy(x => x.Recipe.Title)
        .Select(x => x.Recipe)
        .ToList();

        // Map to DTOs
        var recipeDtos = recipesWithScores.Select(MapToDto).ToList();

        return new SearchRecipesResponseDto(
            Recipes: recipeDtos,
            TotalCount: recipeDtos.Count
        );
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

