namespace PantryCloud.Household.Application.Dtos;

public record SendHouseholdInvitationRequestDto(string ToEmail, string HouseholdId);
public record SendHouseholdInvitationResponseDto(string Code);
public record AcceptHouseholdInvitationRequestDto(string Code);
public record AcceptHouseholdInvitationResponseDto(Guid? HouseholdId, Guid? MemberId, string? MemberEmail, DateTime? JoinedAt);

