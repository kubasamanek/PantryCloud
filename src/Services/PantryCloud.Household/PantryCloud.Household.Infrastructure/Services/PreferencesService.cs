using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Core.Entities;
using PantryCloud.Household.Core.Enums;
using PantryCloud.Household.Core.Errors;
using PantryCloud.Household.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Services;

namespace PantryCloud.Household.Infrastructure.Services;

public class PreferencesService(
    HouseholdDbContext dbContext,
    IUserContext userContext,
    ILogger<PreferencesService> logger)
    : BaseDbContextService<PreferencesService, HouseholdDbContext>(dbContext, userContext, logger), IPreferencesService
{
    public async Task<ErrorOr<GetMyPreferencesResponseDto>> GetMyPreferencesAsync(CancellationToken cancellationToken = default)
    {
        var prefs = await DbContext.MemberPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == UserId, cancellationToken);

        if (prefs is null)
        {
            return HouseholdErrors.PreferencesNotFound;
        }

        return new GetMyPreferencesResponseDto(prefs.DietaryProfile, prefs.ExcludedIngredients);
    }

    public async Task<ErrorOr<UpdateMyPreferencesResponseDto>> UpdateMyPreferencesAsync(UpdateMyPreferencesRequestDto request, CancellationToken cancellationToken = default)
    {
        var isInHousehold = await DbContext.Members.AnyAsync(m => m.UserId == UserId, cancellationToken);
        if (!isInHousehold)
        {
            return HouseholdErrors.UserNotInAnyHousehold;
        }

        var prefs = await DbContext.MemberPreferences.FirstOrDefaultAsync(p => p.UserId == UserId, cancellationToken);
        if (prefs is null)
        {
            prefs = new MemberPreference
            {
                UserId = UserId,
                DietaryProfile = request.DietaryProfile,
                ExcludedIngredients = request.ExcludedIngredients ?? []
            };
            DbContext.MemberPreferences.Add(prefs);
        }
        else
        {
            prefs.DietaryProfile = request.DietaryProfile;
            prefs.ExcludedIngredients = request.ExcludedIngredients ?? [];
        }

        await DbContext.SaveChangesAsync(cancellationToken);

        return new UpdateMyPreferencesResponseDto(prefs.DietaryProfile, prefs.ExcludedIngredients);
    }

    public async Task<ErrorOr<GetHouseholdMembersPreferencesResponseDto>> GetHouseholdMembersPreferencesAsync(CancellationToken cancellationToken = default)
    {
        var member = await DbContext.Members
            .FirstOrDefaultAsync(m => m.UserId == UserId, cancellationToken);
        if (member is null)
        {
            return HouseholdErrors.UserNotInAnyHousehold;
        }

        var memberIds = await DbContext.Members
            .Where(m => m.HouseholdId == member.HouseholdId)
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);

        var preferencesMap = await DbContext.MemberPreferences
            .AsNoTracking()
            .Where(p => memberIds.Contains(p.UserId))
            .ToDictionaryAsync(p => p.UserId, cancellationToken);

        var members = memberIds.Select(uid =>
        {
            var p = preferencesMap.GetValueOrDefault(uid);
            return new MemberPreferencesDto(
                uid,
                p?.DietaryProfile ?? DietaryProfile.None,
                p?.ExcludedIngredients ?? []);
        }).ToList();

        return new GetHouseholdMembersPreferencesResponseDto(members);
    }
}
