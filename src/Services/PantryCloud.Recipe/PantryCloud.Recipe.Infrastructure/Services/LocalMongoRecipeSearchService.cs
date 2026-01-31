using ErrorOr;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Core.Errors;
using PantryCloud.Recipe.Infrastructure.Persistence;
using RecipeEntity = PantryCloud.Recipe.Core.Entities.Recipe;
using IngredientEntity = PantryCloud.Recipe.Core.Entities.Ingredient;

namespace PantryCloud.Recipe.Infrastructure.Services;

public class LocalMongoRecipeSearchService(RecipeDbContext dbContext, ILogger<LocalMongoRecipeSearchService> logger) : IRecipeSearchService
{
    private const int MaxRecipesForRecommendAll = 500;

    public async Task<ErrorOr<SearchRecipesResponseDto>> SearchRecipesAsync(
        SearchRecipesRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var hasPrefs = request.Preferences is not null;
        logger.LogInformation(
            "Searching recipes with {IngredientCount} ingredients, preferences filter: {HasPreferences}",
            request.Ingredients.Count,
            hasPrefs);

        if (request.Ingredients.Count == 0)
        {
            logger.LogWarning("Search recipes failed: ingredients list is empty");
            return RecipeErrors.InvalidSearchRequest;
        }

        var normalizedIngredients = request.Ingredients
            .Select(i => i.Trim().ToLowerInvariant())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .ToList();

        if (normalizedIngredients.Count == 0)
        {
            logger.LogWarning("Search recipes failed: no valid ingredients after normalization");
            return RecipeErrors.InvalidSearchRequest;
        }

        var filterBuilder = Builders<RecipeEntity>.Filter;
        var filters = normalizedIngredients.Select(ingredient =>
            filterBuilder.ElemMatch(
                r => r.Ingredients,
                Builders<IngredientEntity>.Filter.Regex(i => i.Name, new BsonRegularExpression(ingredient, "i"))
            )
        );

        var filter = filterBuilder.Or(filters);

        var recipes = await dbContext.Recipes
            .Find(filter)
            .ToListAsync(cancellationToken);

        logger.LogDebug("MongoDB search returned {Count} recipes matching ingredients", recipes.Count);

        var recipesWithScores = recipes
            .Select(recipe =>
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

        var filtered = ApplyPreferenceFilters(recipesWithScores, request.Preferences);
        if (hasPrefs && filtered.Count != recipesWithScores.Count)
        {
            logger.LogDebug(
                "Preference filters applied: {BeforeCount} -> {AfterCount} recipes",
                recipesWithScores.Count,
                filtered.Count);
        }

        var recipeDtos = filtered.Select(MapToDto).ToList();

        logger.LogInformation("Search completed: returning {TotalCount} recipes", recipeDtos.Count);

        return new SearchRecipesResponseDto(Recipes: recipeDtos, TotalCount: recipeDtos.Count);
    }

    public async Task<ErrorOr<RecommendRecipesResponseDto>> RecommendRecipesAsync(
        RecommendRecipesRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var hints = request.IngredientHints?
            .Select(i => i.Trim().ToLowerInvariant())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .ToList() ?? [];
        var hasPrefs = request.Preferences is not null;
        var limit = Math.Clamp(request.Limit, 1, 100);

        logger.LogInformation(
            "Recommending recipes: ingredient hints={HintCount}, preferences={HasPreferences}, limit={Limit}",
            hints.Count,
            hasPrefs,
            limit);

        List<RecipeEntity> recipes;
        if (hints.Count > 0)
        {
            var filterBuilder = Builders<RecipeEntity>.Filter;
            var filters = hints.Select(ingredient =>
                filterBuilder.ElemMatch(
                    r => r.Ingredients,
                    Builders<IngredientEntity>.Filter.Regex(i => i.Name, new BsonRegularExpression(ingredient, "i"))
                )
            );
            var filter = filterBuilder.Or(filters);
            recipes = await dbContext.Recipes.Find(filter).Limit(MaxRecipesForRecommendAll).ToListAsync(cancellationToken);
            recipes = recipes
                .Select(r => new { Recipe = r, MatchCount = r.Ingredients.Count(i => hints.Contains(i.Name.ToLowerInvariant())) })
                .OrderByDescending(x => x.MatchCount)
                .ThenBy(x => x.Recipe.Title)
                .Select(x => x.Recipe)
                .ToList();
            logger.LogDebug("MongoDB recommend (by hints) returned {Count} recipes", recipes.Count);
        }
        else
        {
            recipes = await dbContext.Recipes
                .Find(FilterDefinition<RecipeEntity>.Empty)
                .Limit(MaxRecipesForRecommendAll)
                .ToListAsync(cancellationToken);
            logger.LogDebug("MongoDB recommend (all) returned {Count} recipes", recipes.Count);
        }

        var filtered = ApplyPreferenceFilters(recipes, request.Preferences);
        if (hasPrefs && filtered.Count != recipes.Count)
        {
            logger.LogDebug(
                "Preference filters applied: {BeforeCount} -> {AfterCount} recipes",
                recipes.Count,
                filtered.Count);
        }

        var recipeDtos = filtered.Take(limit).Select(MapToDto).ToList();

        logger.LogInformation("Recommend completed: returning {Count} recipes", recipeDtos.Count);

        return new RecommendRecipesResponseDto(Recipes: recipeDtos);
    }

    private static List<RecipeEntity> ApplyPreferenceFilters(List<RecipeEntity> recipes, PreferencesFilterDto? prefs)
    {
        if (prefs is null)
            return recipes;

        var result = recipes.AsEnumerable();

        var dietary = prefs.DietaryProfile?.Trim();
        if (!string.IsNullOrEmpty(dietary) && !string.Equals(dietary, "None", StringComparison.OrdinalIgnoreCase))
        {
            var label = dietary.Equals("Vegan", StringComparison.OrdinalIgnoreCase) ? "Vegan"
                : dietary.Equals("Vegetarian", StringComparison.OrdinalIgnoreCase) ? "Vegetarian"
                : null;
            if (label is not null)
                result = result.Where(r => r.DietaryLabels.Any(l => string.Equals(l, label, StringComparison.OrdinalIgnoreCase)));
        }

        var excluded = (prefs.ExcludedIngredients ?? [])
            .Select(i => i.Trim().ToLowerInvariant())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (excluded.Count > 0)
        {
            result = result.Where(r => !r.Ingredients.Any(i =>
                excluded.Contains(i.Name.Trim())));
        }

        return result.ToList();
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
