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

// Invitations
public record SendInvitationRequest(
    [property: JsonPropertyName("toEmail")] string ToEmail,
    [property: JsonPropertyName("householdId")] string HouseholdId);

public record SendInvitationResponse(
    [property: JsonPropertyName("code")] string Code);

public record AcceptInvitationRequest(
    [property: JsonPropertyName("code")] string Code);

public record AcceptInvitationResponse(
    [property: JsonPropertyName("householdId")] Guid? HouseholdId,
    [property: JsonPropertyName("memberId")] Guid? MemberId,
    [property: JsonPropertyName("memberEmail")] string? MemberEmail,
    [property: JsonPropertyName("joinedAt")] DateTime? JoinedAt);

// Leave
public record LeaveHouseholdResponse(
    [property: JsonPropertyName("householdId")] Guid? HouseholdId,
    [property: JsonPropertyName("memberId")] Guid? MemberId,
    [property: JsonPropertyName("memberEmail")] string? MemberEmail,
    [property: JsonPropertyName("leftAt")] DateTime? LeftAt);

// Transfer ownership
public record TransferOwnershipRequest(
    [property: JsonPropertyName("newOwnerUserId")] Guid NewOwnerUserId);

public record TransferOwnershipResponse(
    [property: JsonPropertyName("householdId")] Guid HouseholdId,
    [property: JsonPropertyName("previousOwnerId")] Guid PreviousOwnerId,
    [property: JsonPropertyName("newOwnerId")] Guid NewOwnerId,
    [property: JsonPropertyName("transferredAt")] DateTime TransferredAt);

// Members (from me/members/preferences). Role: 0=Owner, 1=Member (converter accepts number or string from API).
public record MemberWithProfileDto(
    [property: JsonPropertyName("userId")] Guid UserId,
    [property: JsonConverter(typeof(HouseholdRoleJsonConverter))]
    [property: JsonPropertyName("role")] int Role,
    [property: JsonPropertyName("dietaryProfile")] int DietaryProfile,
    [property: JsonPropertyName("excludedIngredients")] List<string> ExcludedIngredients,
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("avatarUrl")] string? AvatarUrl);

public record GetHouseholdMembersResponse(
    [property: JsonPropertyName("members")] List<MemberWithProfileDto> Members);

// Preferences. Backend DietaryProfile enum: 0=None, 1=Vegetarian, 2=Vegan
public record GetMyPreferencesResponse(
    [property: JsonPropertyName("dietaryProfile")] int DietaryProfile,
    [property: JsonPropertyName("excludedIngredients")] List<string> ExcludedIngredients);

public record UpdateMyPreferencesRequest
{
    [JsonPropertyName("dietaryProfile")]
    public int DietaryProfile { get; set; }

    [JsonPropertyName("excludedIngredients")]
    public List<string> ExcludedIngredients { get; set; } = new();
}

public record UpdateMyPreferencesResponse(
    [property: JsonPropertyName("dietaryProfile")] int DietaryProfile,
    [property: JsonPropertyName("excludedIngredients")] List<string> ExcludedIngredients,
    [property: JsonPropertyName("householdId")] Guid HouseholdId,
    [property: JsonPropertyName("userId")] Guid UserId);
