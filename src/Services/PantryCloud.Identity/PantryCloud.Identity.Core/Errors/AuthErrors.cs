using ErrorOr;

namespace PantryCloud.Identity.Core.Errors;

public static class AuthErrors
{
    public static Error RegistrationUserAlreadyExists(string email) => Error.Conflict(
        code: "Auth.Registration.UserAlreadyExists",
        description: $"User with email {email} already exists."
    );

    public static Error LoginInvalidCredentials = Error.Unauthorized(
        code: "Auth.Login.InvalidCredentials",
        description: "Invalid email or password."
    );

    public static Error LoginEmailNotVerified = Error.Unauthorized(
        code: "Auth.Login.EmailNotVerified",
        description: "You need to verify your email first."
    );

    public static Error InvalidRefreshToken = Error.Unauthorized(
        code: "Auth.RefreshToken.InvalidRefreshToken",
        description: "Invalid refresh token."
    );

    public static Error ExpiredRefreshToken = Error.Unauthorized(
        code: "Auth.RefreshToken.ExpiredRefreshToken",
        description: "Expired refresh token."
    );

    public static Error UserDoesNotExist(string email) => Error.NotFound(
        code: "Auth.UserDoesNotExist",
        description: $"User with email {email} not found."
    );

    public static Error TokenNotValid = Error.Unauthorized(
        code: "Auth.TokenInvalid",
        description: $"Token is not valid."
    );

    public static Error TokenExpired = Error.Unauthorized(
        code: "Auth.TokenExpired",
        description: $"Token is expired."
    );

    public static Error TokenAlreadyUsed = Error.Unauthorized(
        code: "Auth.TokenAlreadyUsed",
        description: $"Token has already been used."
    );

    public static Error SessionNotFound = Error.NotFound(
        code: "Auth.Session.NotFound",
        description: "Session not found."
    );

    public static Error SessionAccessDenied = Error.Forbidden(
        code: "Auth.Session.AccessDenied",
        description: "You can only revoke your own sessions."
    );
}