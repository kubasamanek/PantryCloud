using ErrorOr;

namespace PantryCloud.Recipe.Core.Errors;

public static class RecipeErrors
{
    public static Error RecipeNotFound = Error.NotFound(
        code: "Recipe.NotFound",
        description: "Recipe not found."
    );

    public static Error InvalidSearchRequest = Error.Validation(
        code: "Recipe.InvalidSearchRequest",
        description: "Invalid search request. At least one ingredient is required."
    );
}


