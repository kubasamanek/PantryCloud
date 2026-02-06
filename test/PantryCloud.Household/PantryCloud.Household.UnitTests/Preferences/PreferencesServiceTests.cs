using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Core.Entities;
using PantryCloud.Household.Core.Enums;
using PantryCloud.Household.Core.Errors;
using PantryCloud.Household.Infrastructure.Services;
using Shouldly;

namespace PantryCloud.Household.UnitTests.Preferences;

public class PreferencesServiceTests
{
    private readonly ILogger<PreferencesService> _logger = TestHelper.MockLogger<PreferencesService>();

    [Fact]
    public async Task GetMyPreferencesAsync_ShouldReturnNotFound_WhenNoPreferences()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetMyPreferencesAsync_ShouldReturnNotFound_WhenNoPreferences));
        db.Members.Add(new HouseholdMember
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            Role = HouseholdRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new PreferencesService(db, TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email), _logger);

        var result = await service.GetMyPreferencesAsync(CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.PreferencesNotFound);
    }

    [Fact]
    public async Task GetMyPreferencesAsync_ShouldReturnPreferences_WhenExists()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetMyPreferencesAsync_ShouldReturnPreferences_WhenExists));
        db.MemberPreferences.Add(new MemberPreference
        {
            UserId = Constants.User.Id,
            DietaryProfile = DietaryProfile.Vegetarian,
            ExcludedIngredients = ["nuts", "dairy"]
        });
        await db.SaveChangesAsync();

        var service = new PreferencesService(db, TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email), _logger);

        var result = await service.GetMyPreferencesAsync(CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.DietaryProfile.ShouldBe(DietaryProfile.Vegetarian);
        result.Value.ExcludedIngredients.ShouldBe(["nuts", "dairy"]);
    }

    [Fact]
    public async Task UpdateMyPreferencesAsync_ShouldCreatePreferences_WhenMemberHasNone()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdateMyPreferencesAsync_ShouldCreatePreferences_WhenMemberHasNone));
        db.Members.Add(new HouseholdMember
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            Role = HouseholdRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new PreferencesService(db, TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email), _logger);
        var request = new UpdateMyPreferencesRequestDto(DietaryProfile.Vegan, ["gluten"]);

        var result = await service.UpdateMyPreferencesAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.DietaryProfile.ShouldBe(DietaryProfile.Vegan);
        result.Value.ExcludedIngredients.ShouldBe(["gluten"]);
        result.Value.HouseholdId.ShouldBe(Constants.Household.Id);
        result.Value.UserId.ShouldBe(Constants.User.Id);

        var prefs = await db.MemberPreferences.FirstOrDefaultAsync(p => p.UserId == Constants.User.Id);
        prefs.ShouldNotBeNull();
        prefs!.DietaryProfile.ShouldBe(DietaryProfile.Vegan);
        prefs.ExcludedIngredients.ShouldBe(["gluten"]);
    }

    [Fact]
    public async Task UpdateMyPreferencesAsync_ShouldUpdatePreferences_WhenAlreadyExist()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdateMyPreferencesAsync_ShouldUpdatePreferences_WhenAlreadyExist));
        db.Members.Add(new HouseholdMember
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            Role = HouseholdRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        db.MemberPreferences.Add(new MemberPreference
        {
            UserId = Constants.User.Id,
            DietaryProfile = DietaryProfile.None,
            ExcludedIngredients = []
        });
        await db.SaveChangesAsync();

        var service = new PreferencesService(db, TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email), _logger);
        var request = new UpdateMyPreferencesRequestDto(DietaryProfile.Vegetarian, ["shellfish"]);

        var result = await service.UpdateMyPreferencesAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.DietaryProfile.ShouldBe(DietaryProfile.Vegetarian);
        result.Value.ExcludedIngredients.ShouldBe(["shellfish"]);

        var prefs = await db.MemberPreferences.FirstAsync(p => p.UserId == Constants.User.Id);
        prefs.DietaryProfile.ShouldBe(DietaryProfile.Vegetarian);
        prefs.ExcludedIngredients.ShouldBe(["shellfish"]);
    }

    [Fact]
    public async Task UpdateMyPreferencesAsync_ShouldReturnError_WhenUserNotInHousehold()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdateMyPreferencesAsync_ShouldReturnError_WhenUserNotInHousehold));
        var service = new PreferencesService(db, TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email), _logger);
        var request = new UpdateMyPreferencesRequestDto(DietaryProfile.Vegan, null);

        var result = await service.UpdateMyPreferencesAsync(request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.UserNotInAnyHousehold);
    }

    [Fact]
    public async Task GetHouseholdMembersPreferencesAsync_ShouldReturnMembersWithPreferences()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetHouseholdMembersPreferencesAsync_ShouldReturnMembersWithPreferences));
        db.Members.AddRange(
            new HouseholdMember { UserId = Constants.User.Id, HouseholdId = Constants.Household.Id, Role = HouseholdRole.Owner, JoinedAt = DateTime.UtcNow },
            new HouseholdMember { UserId = Constants.MemberIds.NewOwner, HouseholdId = Constants.Household.Id, Role = HouseholdRole.Member, JoinedAt = DateTime.UtcNow }
        );
        db.MemberPreferences.Add(new MemberPreference
        {
            UserId = Constants.User.Id,
            DietaryProfile = DietaryProfile.Vegetarian,
            ExcludedIngredients = ["nuts"]
        });
        db.MemberProfiles.Add(new MemberProfile
        {
            UserId = Constants.User.Id,
            DisplayName = Constants.Profile.DisplayName,
            AvatarUrl = Constants.Profile.AvatarUrl
        });
        await db.SaveChangesAsync();

        var service = new PreferencesService(db, TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email), _logger);

        var result = await service.GetHouseholdMembersPreferencesAsync(CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Members.Count.ShouldBe(2);
        var owner = result.Value.Members.First(m => m.UserId == Constants.User.Id);
        owner.DietaryProfile.ShouldBe(DietaryProfile.Vegetarian);
        owner.ExcludedIngredients.ShouldBe(["nuts"]);
        owner.DisplayName.ShouldBe(Constants.Profile.DisplayName);
        owner.AvatarUrl.ShouldBe(Constants.Profile.AvatarUrl);
        var member = result.Value.Members.First(m => m.UserId == Constants.MemberIds.NewOwner);
        member.DietaryProfile.ShouldBe(DietaryProfile.None);
        member.ExcludedIngredients.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetHouseholdMembersPreferencesAsync_ShouldReturnError_WhenUserNotInHousehold()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetHouseholdMembersPreferencesAsync_ShouldReturnError_WhenUserNotInHousehold));
        var service = new PreferencesService(db, TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email), _logger);

        var result = await service.GetHouseholdMembersPreferencesAsync(CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.UserNotInAnyHousehold);
    }
}
