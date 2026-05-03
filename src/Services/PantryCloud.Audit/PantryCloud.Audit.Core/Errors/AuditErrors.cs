using ErrorOr;

namespace PantryCloud.Audit.Core.Errors;

public static class AuditErrors
{
    public static Error UserNotInHousehold => Error.Forbidden(
        code: "Audit.UserNotInHousehold",
        description: "You can only view audit for households you are a member of.");
}
