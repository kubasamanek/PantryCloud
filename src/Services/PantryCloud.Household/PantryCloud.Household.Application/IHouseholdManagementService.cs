using ErrorOr;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application;

public interface IHouseholdManagementService
{
    Task<ErrorOr<GetCurrentHouseholdResponseDto>> GetCurrentHousehold(CancellationToken cancellationToken);
    Task<ErrorOr<GetHouseholdByUserIdResponseDto>> GetHouseholdByUserId(GetHouseholdByUserIdRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<CreateHouseholdResponseDto>> CreateHousehold(CreateHouseholdRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<LeaveHouseholdResponseDto>> LeaveHousehold(LeaveHouseholdRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<KickMemberResponseDto>> KickMemberAsync(KickMemberRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<TransferOwnershipResponseDto>> TransferOwnershipAsync(TransferOwnershipRequestDto request, CancellationToken cancellationToken);
}