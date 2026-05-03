using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PantryCloud.Web.Services.Profile;

public record GetMyProfileResponse(
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("avatarUrl")] string? AvatarUrl);

public record UpdateProfileRequest
{
    [Required(ErrorMessage = "Display name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Display name must be between 1 and 100 characters")]
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = "";

    [StringLength(500, ErrorMessage = "Avatar URL must be at most 500 characters")]
    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }
}

public record UpdateProfileResponse(
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("avatarUrl")] string? AvatarUrl);
