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
    string Password,
    string? DeviceName = null
);

public record LoginResponseDto(string AccessToken, string RefreshToken, Guid? SessionId = null);

public record RegisterResponseDto(string UserId, string VerifyEmailToken);

public record RefreshTokenRequestDto(
    [Required(ErrorMessage = "Refresh token is required")]
    string RefreshToken,
    string? DeviceName = null
);

public record RefreshTokenResponseDto(string AccessToken, string RefreshToken, Guid? SessionId = null);

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

public record SessionDto(Guid Id, DateTime CreatedAt, DateTime? LastUsedAt, string? DeviceName, bool IsCurrent);

public record ListSessionsResponseDto(List<SessionDto> Sessions);

public record RevokeSessionRequestDto(Guid SessionId);

public record LogoutRequestDto(
    [Required(ErrorMessage = "Refresh token is required")]
    string RefreshToken);