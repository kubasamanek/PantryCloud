using ErrorOr;

namespace PantryCloud.Household.Core.Errors;

public static class HouseholdErrors
{
    public static Error UserAlreadyInHousehold => Error.Conflict(
        code: "Household.Creation.UserAlreadyInHousehold",
        description: "User is a member of a household and cannot create a new one."
    );

    public static Error UserNotInAnyHousehold => Error.NotFound(
        code: "Household.Creation.UserNotInAnyHousehold",
        description: "User is not in any household."
    );

    public static Error OwnerCannotLeave => Error.Forbidden(
        code:  "Household.Owner.CannotLeave",
        description: "Owner is a member of a household and cannot leave."
    );

    public static Error PreferencesNotFound => Error.NotFound(
        code: "Household.Preferences.NotFound",
        description: "No preferences set for this user."
    );

    public static Error ProfileNotFound => Error.NotFound(
        code: "Household.Profile.NotFound",
        description: "No profile set for this user."
    );
}