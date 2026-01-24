using System.ComponentModel.DataAnnotations;

namespace PantryCloud.Identity.Application.DTOs;

public record RegisterRequestDto(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    string Password
);

public record LoginRequestDto(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,
    
    [Required(ErrorMessage = "Password is required")]
    string Password
);

public record LoginResponseDto(string AccessToken, string RefreshToken);

public record RegisterResponseDto(string UserId, string VerifyEmailToken);

public record RefreshTokenRequestDto(
    [Required(ErrorMessage = "Refresh token is required")]
    string RefreshToken
);

public record RefreshTokenResponseDto(string AccessToken, string RefreshToken);

public record ForgotPasswordRequestDto(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email
);

public record ForgotPasswordResponseDto(string Token, string Url);

public record ResetPasswordRequestDto(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,
    
    [Required(ErrorMessage = "Token is required")]
    string Token,
    
    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    string NewPassword
);

public record ResetPasswordResponseDto();

public record VerifyEmailRequestDto(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,
    
    [Required(ErrorMessage = "Token is required")]
    string Token
);

public record VerifyEmailResponseDto();