using PantryCloud.Household.Core.Enums;

namespace PantryCloud.Household.Application.Dtos;

public record GetMyPreferencesRequestDto;

public record GetMyPreferencesResponseDto(
    DietaryProfile DietaryProfile,
    List<string> ExcludedIngredients);

public record UpdateMyPreferencesRequestDto(
    DietaryProfile DietaryProfile,
    List<string>? ExcludedIngredients = null);

public record UpdateMyPreferencesResponseDto(
    DietaryProfile DietaryProfile,
    List<string> ExcludedIngredients,
    Guid HouseholdId,
    Guid UserId);

public record MemberPreferencesDto(
    Guid UserId,
    DietaryProfile DietaryProfile,
    List<string> ExcludedIngredients);

public record MemberWithProfileDto(
    Guid UserId,
    DietaryProfile DietaryProfile,
    List<string> ExcludedIngredients,
    string? DisplayName,
    string? AvatarUrl);

public record GetHouseholdMembersPreferencesRequestDto;

public record GetHouseholdMembersPreferencesResponseDto(
    List<MemberWithProfileDto> Members);
