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
        code: "Household.Owner.CannotLeave",
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

    public static Error UserNotOwner => Error.Forbidden(
        code: "Household.Role.UserNotOwner",
        description: "Only the household owner can perform this action."
    );

    public static Error CannotKickSelf => Error.Validation(
        code: "Household.Kick.CannotKickSelf",
        description: "You cannot kick yourself from the household."
    );

    public static Error CannotKickOwner => Error.Validation(
        code: "Household.Kick.CannotKickOwner",
        description: "You cannot kick the household owner."
    );

    public static Error NewOwnerMustBeMember => Error.Validation(
        code: "Household.Transfer.NewOwnerMustBeMember",
        description: "The new owner must be an existing member of the household."
    );

    public static Error CannotTransferToSelf => Error.Validation(
        code: "Household.Transfer.CannotTransferToSelf",
        description: "You cannot transfer ownership to yourself."
    );

    public static Error MemberNotFoundInHousehold => Error.NotFound(
        code: "Household.Member.NotFound",
        description: "The specified user is not a member of this household."
    );
}