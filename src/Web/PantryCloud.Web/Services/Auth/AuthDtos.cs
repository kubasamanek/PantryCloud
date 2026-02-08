using System.ComponentModel.DataAnnotations;

namespace PantryCloud.Web.Services.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = "";
    public string? DeviceName { get; set; }
}

public record LoginResponse(string AccessToken, string RefreshToken);

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = "";
}

public record RegisterResponse(string UserId, string VerifyEmailToken);

public record RefreshTokenRequest(string RefreshToken);

public record RefreshTokenResponse(string AccessToken, string RefreshToken);

public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = "";
}

public record ForgotPasswordResponse(string Token, string Url);

public record VerifyEmailRequest(string Email, string Token);

public record VerifyEmailResponse();

public class ResetPasswordRequest
{
    public string Email { get; set; } = "";
    public string Token { get; set; } = "";

    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string NewPassword { get; set; } = "";
}
