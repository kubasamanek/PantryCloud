using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PantryCloud.Web.Services.Household;

public record GetCurrentHouseholdResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name);

public record CreateHouseholdRequest
{
    [Required(ErrorMessage = "Household name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

public record CreateHouseholdResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("ownerId")] Guid OwnerId,
    [property: JsonPropertyName("ownerEmail")] string OwnerEmail,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt);
