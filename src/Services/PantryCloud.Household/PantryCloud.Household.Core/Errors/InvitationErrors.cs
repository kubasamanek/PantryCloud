using ErrorOr;

namespace PantryCloud.Household.Core.Errors;

public static class InvitationErrors
{
    public static Error UserNotInHousehold => Error.Unauthorized(
        code: "Household.Invitations.UserNotInHousehold",
        description: "User is not in household."
    );

    public static Error UserNotHouseholdOwner => Error.Unauthorized(
        code: "Household.Invitations.UserNotNotHouseholdOwner",
        description: "Member of household cannot do this."
    );

    public static Error UserNotHouseholdMember => Error.Unauthorized(
        code: "Household.Invitations.UserNotNotHouseholdMember",
        description: "User is not in this household."
    );

    public static Error InvalidInvitation => Error.Validation(
        code: "Household.Invitations.InvalidInvitation",
        description: "Invalid invitation."
    );

    public static Error ExpiredInvitation => Error.Unauthorized(
        code: "Household.Invitations.ExpiredInvitation",
        description: "Expired invitation."
    );

    public static Error UsedInvitation => Error.Unauthorized(
        code: "Household.Invitations.UsedInvitation",
        description: "Used invitation."
    );

    public static Error InvitationNotForUser(string email) => Error.Unauthorized(
        code: "Household.Invitations.InvitationNotForUser",
        description: $"Invitation is for {email}."
    );

    public static Error OwnerCannotAcceptInvitation => Error.Forbidden(
        code: "Household.Invitations.OwnerCannotAccept",
        description: "Household owner must transfer ownership before joining another household."
    );
}