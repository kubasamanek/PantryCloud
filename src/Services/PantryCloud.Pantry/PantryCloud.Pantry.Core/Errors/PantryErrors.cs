using ErrorOr;

namespace PantryCloud.Pantry.Core.Errors;

public static class PantryErrors
{
    public static Error PantryItemNotFound = Error.NotFound(
        code: "Pantry.ItemNotFound",
        description: "Pantry item not found."
    );

    public static Error PantryItemConcurrencyConflict = Error.Conflict(
        code: "Pantry.ConcurrencyConflict",
        description: "The pantry item was modified by another user. Please refresh and try again."
    );

    public static Error HouseholdNotFound = Error.NotFound(
        code: "Pantry.HouseholdNotFound",
        description: "Household not found."
    );

    public static Error UnauthorizedAccess = Error.Forbidden(
        code: "Pantry.UnauthorizedAccess",
        description: "You do not have access to this household or the household ID could not be validated."
    );
}

