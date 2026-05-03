using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Core.Entities;
using PantryCloud.Household.Core.Errors;
using PantryCloud.Household.Infrastructure.Services;
using Shouldly;

namespace PantryCloud.Household.UnitTests.Profile;

public class ProfileServiceTests
{
    private readonly ILogger<ProfileService> _logger = TestHelper.MockLogger<ProfileService>();

    [Fact]
    public async Task GetMyProfileAsync_ReturnsNotFound_WhenNoProfile()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetMyProfileAsync_ReturnsNotFound_WhenNoProfile));
        var service = new ProfileService(db, TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email), _logger);

        var result = await service.GetMyProfileAsync(CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.ProfileNotFound);
    }

    [Fact]
    public async Task GetMyProfileAsync_ReturnsProfile_WhenExists()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetMyProfileAsync_ReturnsProfile_WhenExists));
        db.MemberProfiles.Add(new MemberProfile
        {
            UserId = Constants.User.Id,
            DisplayName = Constants.Profile.DisplayName,
            AvatarUrl = Constants.Profile.AvatarUrl
        });
        await db.SaveChangesAsync();

        var service = new ProfileService(db, TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email), _logger);

        var result = await service.GetMyProfileAsync(CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.DisplayName.ShouldBe(Constants.Profile.DisplayName);
        result.Value.AvatarUrl.ShouldBe(Constants.Profile.AvatarUrl);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_CreatesProfile_WhenNoneExists()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdateMyProfileAsync_CreatesProfile_WhenNoneExists));
        var service = new ProfileService(db, TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email), _logger);

        var request = new UpdateMyProfileRequestDto(Constants.Profile.NewDisplayName, Constants.Profile.AvatarUrl);
        var result = await service.UpdateMyProfileAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.DisplayName.ShouldBe(Constants.Profile.NewDisplayName);
        result.Value.AvatarUrl.ShouldBe(Constants.Profile.AvatarUrl);

        var profile = await db.MemberProfiles.SingleOrDefaultAsync(p => p.UserId == Constants.User.Id);
        profile.ShouldNotBeNull();
        profile!.DisplayName.ShouldBe(Constants.Profile.NewDisplayName);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_UpdatesExisting_WhenProfileExists()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdateMyProfileAsync_UpdatesExisting_WhenProfileExists));
        db.MemberProfiles.Add(new MemberProfile
        {
            UserId = Constants.User.Id,
            DisplayName = Constants.Profile.OldDisplayName,
            AvatarUrl = Constants.Profile.OldAvatarUrl
        });
        await db.SaveChangesAsync();

        var service = new ProfileService(db, TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email), _logger);

        var request = new UpdateMyProfileRequestDto(Constants.Profile.UpdatedDisplayName, null);
        var result = await service.UpdateMyProfileAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.DisplayName.ShouldBe(Constants.Profile.UpdatedDisplayName);
        result.Value.AvatarUrl.ShouldBeNull();

        var profile = await db.MemberProfiles.SingleAsync(p => p.UserId == Constants.User.Id);
        profile.DisplayName.ShouldBe(Constants.Profile.UpdatedDisplayName);
        profile.AvatarUrl.ShouldBeNull();
    }
}
