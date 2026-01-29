using ErrorOr;

namespace PantryCloud.ShoppingList.Core.Errors;

public static class ShoppingListErrors
{
    public static Error ShoppingListNotFound = Error.NotFound(
        code: "ShoppingList.ListNotFound",
        description: "Shopping list not found."
    );

    public static Error ShoppingListItemNotFound = Error.NotFound(
        code: "ShoppingList.ItemNotFound",
        description: "Shopping list item not found."
    );

    public static Error ShoppingListConcurrencyConflict = Error.Conflict(
        code: "ShoppingList.ConcurrencyConflict",
        description: "The shopping list item was modified by another user. Please refresh and try again."
    );

    public static Error HouseholdNotFound = Error.NotFound(
        code: "ShoppingList.HouseholdNotFound",
        description: "Household not found or you are not a member."
    );

    public static Error UnauthorizedAccess = Error.Forbidden(
        code: "ShoppingList.UnauthorizedAccess",
        description: "You do not have access to this shopping list."
    );

    public static Error DuplicateListName = Error.Conflict(
        code: "ShoppingList.DuplicateListName",
        description: "A shopping list with this name already exists in your household."
    );
}


