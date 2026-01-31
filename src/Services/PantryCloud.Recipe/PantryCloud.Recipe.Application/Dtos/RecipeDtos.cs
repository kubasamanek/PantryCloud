using PantryCloud.Recipe.Core.Entities;
using PantryCloud.Recipe.Core.Enums;

namespace PantryCloud.Recipe.Application.Dtos;

public record PreferencesFilterDto(
    string? DietaryProfile = null,
    List<string>? ExcludedIngredients = null);

public record SearchRecipesRequestDto(
    List<string> Ingredients,
    PreferencesFilterDto? Preferences = null);

public record SearchRecipesResponseDto(
    List<RecipeDto> Recipes,
    int TotalCount);

public record GetRecipeRequestDto(Guid Id);

public record GetRecipeResponseDto(RecipeDto Recipe);

public record SeedRecipesRequestDto(int Count = 10);

public record SeedRecipesResponseDto(int RecipesCreated);

public record RecommendRecipesRequestDto(
    List<string>? IngredientHints = null,
    PreferencesFilterDto? Preferences = null,
    int Limit = 20);

public record RecommendRecipesResponseDto(
    List<RecipeDto> Recipes);

public record RecipeDto(
    Guid Id,
    string Title,
    string? Description,
    List<IngredientDto> Ingredients,
    List<RecipeStepDto> Steps,
    List<string> Tags,
    List<string> DietaryLabels,
    RecipeSource Source,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record IngredientDto(
    string Name,
    decimal Quantity,
    PantryCloud.SharedKernel.Enums.Unit Unit,
    bool IsOptional);

public record RecipeStepDto(
    int StepNumber,
    string Instruction,
    int? DurationMinutes,
    string? ImageUrl);


