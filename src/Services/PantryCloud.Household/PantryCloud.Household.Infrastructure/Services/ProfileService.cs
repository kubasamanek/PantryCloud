using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Core.Entities;
using PantryCloud.Household.Core.Errors;
using PantryCloud.Household.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Services;

namespace PantryCloud.Household.Infrastructure.Services;

public class ProfileService(
    HouseholdDbContext dbContext,
    IUserContext userContext,
    ILogger<ProfileService> logger)
    : BaseDbContextService<ProfileService, HouseholdDbContext>(dbContext, userContext, logger), IProfileService
{
    public async Task<ErrorOr<GetMyProfileResponseDto>> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Getting profile for user {UserId}", UserId);
        var profile = await DbContext.MemberProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == UserId, cancellationToken);

        if (profile is null)
        {
            Logger.LogInformation("No profile found for user {UserId}", UserId);
            return HouseholdErrors.ProfileNotFound;
        }

        Logger.LogDebug("Profile retrieved for user {UserId}, DisplayName={DisplayName}", UserId, profile.DisplayName);
        return new GetMyProfileResponseDto(profile.DisplayName, profile.AvatarUrl);
    }

    public async Task<ErrorOr<UpdateMyProfileResponseDto>> UpdateMyProfileAsync(UpdateMyProfileRequestDto request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Updating profile for user {UserId}, DisplayName={DisplayName}", UserId, request.DisplayName);
        var profile = await DbContext.MemberProfiles.FirstOrDefaultAsync(p => p.UserId == UserId, cancellationToken);

        if (profile is null)
        {
            profile = new MemberProfile
            {
                UserId = UserId,
                DisplayName = request.DisplayName,
                AvatarUrl = request.AvatarUrl
            };
            DbContext.MemberProfiles.Add(profile);
            Logger.LogInformation("Created new profile for user {UserId}", UserId);
        }
        else
        {
            profile.DisplayName = request.DisplayName;
            profile.AvatarUrl = request.AvatarUrl;
            Logger.LogInformation("Updated existing profile for user {UserId}", UserId);
        }

        await DbContext.SaveChangesAsync(cancellationToken);

        return new UpdateMyProfileResponseDto(profile.DisplayName, profile.AvatarUrl);
    }
}
