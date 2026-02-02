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
    private static readonly Guid UserId = Guid.NewGuid();
    private readonly ILogger<ProfileService> _logger = TestHelper.MockLogger<ProfileService>();

    [Fact]
    public async Task GetMyProfileAsync_ReturnsNotFound_WhenNoProfile()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetMyProfileAsync_ReturnsNotFound_WhenNoProfile));
        var service = new ProfileService(db, TestHelper.CreateMockUserContext(UserId, "test@example.com"), _logger);

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
            UserId = UserId,
            DisplayName = "Test User",
            AvatarUrl = "https://example.com/avatar.png"
        });
        await db.SaveChangesAsync();

        var service = new ProfileService(db, TestHelper.CreateMockUserContext(UserId, "test@example.com"), _logger);

        var result = await service.GetMyProfileAsync(CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.DisplayName.ShouldBe("Test User");
        result.Value.AvatarUrl.ShouldBe("https://example.com/avatar.png");
    }

    [Fact]
    public async Task UpdateMyProfileAsync_CreatesProfile_WhenNoneExists()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdateMyProfileAsync_CreatesProfile_WhenNoneExists));
        var service = new ProfileService(db, TestHelper.CreateMockUserContext(UserId, "test@example.com"), _logger);

        var request = new UpdateMyProfileRequestDto("My Display Name", "https://example.com/avatar.png");
        var result = await service.UpdateMyProfileAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.DisplayName.ShouldBe("My Display Name");
        result.Value.AvatarUrl.ShouldBe("https://example.com/avatar.png");

        var profile = await db.MemberProfiles.SingleOrDefaultAsync(p => p.UserId == UserId);
        profile.ShouldNotBeNull();
        profile!.DisplayName.ShouldBe("My Display Name");
    }

    [Fact]
    public async Task UpdateMyProfileAsync_UpdatesExisting_WhenProfileExists()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdateMyProfileAsync_UpdatesExisting_WhenProfileExists));
        db.MemberProfiles.Add(new MemberProfile
        {
            UserId = UserId,
            DisplayName = "Old Name",
            AvatarUrl = "https://old.com/avatar.png"
        });
        await db.SaveChangesAsync();

        var service = new ProfileService(db, TestHelper.CreateMockUserContext(UserId, "test@example.com"), _logger);

        var request = new UpdateMyProfileRequestDto("New Name", null);
        var result = await service.UpdateMyProfileAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.DisplayName.ShouldBe("New Name");
        result.Value.AvatarUrl.ShouldBeNull();

        var profile = await db.MemberProfiles.SingleAsync(p => p.UserId == UserId);
        profile.DisplayName.ShouldBe("New Name");
        profile.AvatarUrl.ShouldBeNull();
    }
}
