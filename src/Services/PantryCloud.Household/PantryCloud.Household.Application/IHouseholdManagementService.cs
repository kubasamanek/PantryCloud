using ErrorOr;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application;

public interface IHouseholdManagementService
{
    public Task<ErrorOr<GetCurrentHouseholdResponseDto>> GetCurrentHousehold(CancellationToken cancellationToken);
    public Task<ErrorOr<GetHouseholdByUserIdResponseDto>> GetHouseholdByUserId(GetHouseholdByUserIdRequestDto request, CancellationToken cancellationToken);
    public Task<ErrorOr<CreateHouseholdResponseDto>> CreateHousehold(CreateHouseholdRequestDto request, CancellationToken cancellationToken);
    public Task<ErrorOr<LeaveHouseholdResponseDto>> LeaveHousehold(LeaveHouseholdRequestDto request, CancellationToken cancellationToken);
}