using ErrorOr;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application;

public interface IPreferencesService
{
    Task<ErrorOr<GetMyPreferencesResponseDto>> GetMyPreferencesAsync(CancellationToken cancellationToken = default);
    Task<ErrorOr<UpdateMyPreferencesResponseDto>> UpdateMyPreferencesAsync(UpdateMyPreferencesRequestDto request, CancellationToken cancellationToken = default);
    Task<ErrorOr<GetHouseholdMembersPreferencesResponseDto>> GetHouseholdMembersPreferencesAsync(CancellationToken cancellationToken = default);
}
