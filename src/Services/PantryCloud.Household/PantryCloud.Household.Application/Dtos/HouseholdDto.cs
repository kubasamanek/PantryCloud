namespace PantryCloud.Household.Application.Dtos;

public record CreateHouseholdRequestDto(string Name);

public record CreateHouseholdResponseDto(Guid Id, string Name, Guid OwnerId, string OwnerEmail, DateTime CreatedAt);

public record LeaveHouseholdRequestDto();

public record LeaveHouseholdResponseDto(Guid? HouseholdId, Guid? MemberId, string? MemberEmail, DateTime? LeftAt);

public record GetCurrentHouseholdRequestDto();
public record GetCurrentHouseholdResponseDto(Guid Id, string Name);

public record GetHouseholdByUserIdRequestDto(Guid UserId);
public record GetHouseholdByUserIdResponseDto(Guid Id, string Name);
