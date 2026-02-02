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
        var profile = await DbContext.MemberProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == UserId, cancellationToken);

        if (profile is null)
        {
            return HouseholdErrors.ProfileNotFound;
        }

        return new GetMyProfileResponseDto(profile.DisplayName, profile.AvatarUrl);
    }

    public async Task<ErrorOr<UpdateMyProfileResponseDto>> UpdateMyProfileAsync(UpdateMyProfileRequestDto request, CancellationToken cancellationToken = default)
    {
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
        }
        else
        {
            profile.DisplayName = request.DisplayName;
            profile.AvatarUrl = request.AvatarUrl;
        }

        await DbContext.SaveChangesAsync(cancellationToken);

        return new UpdateMyProfileResponseDto(profile.DisplayName, profile.AvatarUrl);
    }
}
