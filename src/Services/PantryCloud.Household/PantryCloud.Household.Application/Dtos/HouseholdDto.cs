namespace PantryCloud.Household.Application.Dtos;

public record CreateHouseholdRequestDto(string Name);

public record CreateHouseholdResponseDto(Guid Id, string Name);

public record LeaveHouseholdRequestDto();

public record LeaveHouseholdResponseDto();

public record GetCurrentHouseholdRequestDto();
public record GetCurrentHouseholdResponseDto(Guid Id, string Name);