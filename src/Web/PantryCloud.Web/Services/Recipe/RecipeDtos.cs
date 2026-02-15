using System.Text.Json.Serialization;

namespace PantryCloud.Web.Services.Recipe;

public record PreferencesFilterDto
{
    [JsonPropertyName("dietaryProfile")] public string? DietaryProfile { get; set; }
    [JsonPropertyName("excludedIngredients")] public List<string>? ExcludedIngredients { get; set; }
}

public record RecommendRecipesRequest
{
    [JsonPropertyName("ingredientHints")] public List<string>? IngredientHints { get; set; }
    [JsonPropertyName("preferences")] public PreferencesFilterDto? Preferences { get; set; }
    [JsonPropertyName("limit")] public int Limit { get; set; } = 20;
}

public record SearchRecipesRequest
{
    [JsonPropertyName("ingredients")] public List<string> Ingredients { get; set; } = [];
    [JsonPropertyName("preferences")] public PreferencesFilterDto? Preferences { get; set; }
}

public record RecipeDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("title")] public string Title { get; init; } = "";
    [JsonPropertyName("description")] public string? Description { get; init; }
    [JsonPropertyName("ingredients")] public IReadOnlyList<RecipeIngredientDto> Ingredients { get; init; } = [];
    [JsonPropertyName("steps")] public IReadOnlyList<RecipeStepDto> Steps { get; init; } = [];
    [JsonPropertyName("tags")] public IReadOnlyList<string> Tags { get; init; } = [];
    [JsonPropertyName("dietaryLabels")] public IReadOnlyList<string> DietaryLabels { get; init; } = [];
    [JsonPropertyName("source")] public int Source { get; init; }
    [JsonPropertyName("prepTimeMinutes")] public int PrepTimeMinutes { get; init; }
    [JsonPropertyName("cookTimeMinutes")] public int CookTimeMinutes { get; init; }
    [JsonPropertyName("servings")] public int Servings { get; init; }
    [JsonPropertyName("imageUrl")] public string? ImageUrl { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
    [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; init; }
}

public record RecipeIngredientDto
{
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("quantity")] public decimal Quantity { get; init; }
    [JsonPropertyName("unit")] public int Unit { get; init; }
    [JsonPropertyName("isOptional")] public bool IsOptional { get; init; }
}

public record RecipeStepDto
{
    [JsonPropertyName("stepNumber")] public int StepNumber { get; init; }
    [JsonPropertyName("instruction")] public string Instruction { get; init; } = "";
    [JsonPropertyName("durationMinutes")] public int? DurationMinutes { get; init; }
    [JsonPropertyName("imageUrl")] public string? ImageUrl { get; init; }
}

public record RecommendRecipesResponse
{
    [JsonPropertyName("recipes")] public IReadOnlyList<RecipeDto> Recipes { get; init; } = [];
}

public record SearchRecipesResponse
{
    [JsonPropertyName("recipes")] public IReadOnlyList<RecipeDto> Recipes { get; init; } = [];
    [JsonPropertyName("totalCount")] public int TotalCount { get; init; }
}

public record GetRecipeResponse
{
    [JsonPropertyName("recipe")] public RecipeDto Recipe { get; init; } = null!;
}
