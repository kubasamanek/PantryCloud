using ErrorOr;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application;

public interface IProfileService
{
    Task<ErrorOr<GetMyProfileResponseDto>> GetMyProfileAsync(CancellationToken cancellationToken = default);
    Task<ErrorOr<UpdateMyProfileResponseDto>> UpdateMyProfileAsync(UpdateMyProfileRequestDto request, CancellationToken cancellationToken = default);
}
