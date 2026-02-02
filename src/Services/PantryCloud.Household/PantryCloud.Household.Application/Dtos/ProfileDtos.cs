using System.ComponentModel.DataAnnotations;

namespace PantryCloud.Household.Application.Dtos;

public record GetMyProfileRequestDto;

public record GetMyProfileResponseDto(string DisplayName, string? AvatarUrl);

public record UpdateMyProfileRequestDto(
    [Required(ErrorMessage = "Display name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Display name must be between 1 and 100 characters")]
    string DisplayName,
    [StringLength(500, ErrorMessage = "Avatar URL must be at most 500 characters")]
    string? AvatarUrl = null);

public record UpdateMyProfileResponseDto(string DisplayName, string? AvatarUrl);
