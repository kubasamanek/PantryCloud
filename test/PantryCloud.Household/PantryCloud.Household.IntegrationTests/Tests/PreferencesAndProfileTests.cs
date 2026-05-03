using System.Net;
using System.Net.Http.Json;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Core.Enums;
using PantryCloud.Household.IntegrationTests.Constants;
using PantryCloud.Household.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Household.IntegrationTests.Tests;

public class PreferencesAndProfileTests(HouseholdTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task GetMyPreferences_Returns200_WithDefaults_WhenMemberHasHousehold()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);

        await client.PostAsJsonAsync(TestConstants.Endpoints.CreateHousehold, new CreateHouseholdRequestDto(TestConstants.TestData.HouseholdName));
        await client.PutAsJsonAsync(TestConstants.Endpoints.UpdateMyPreferences, new UpdateMyPreferencesRequestDto(DietaryProfile.None, new List<string>()));
        var response = await client.GetAsync(TestConstants.Endpoints.GetMyPreferences);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await GetFromJsonAsync<GetMyPreferencesResponseDto>(response);
        body.ShouldNotBeNull();
        body.DietaryProfile.ShouldBe(DietaryProfile.None);
        body.ExcludedIngredients.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetMyPreferences_Returns404_WhenUserHasNoHousehold()
    {
        await ResetAsync();
        var client = CreateClientWithToken(Guid.NewGuid());

        var response = await client.GetAsync(TestConstants.Endpoints.GetMyPreferences);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateMyPreferences_Returns200_AndPersists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);

        await client.PostAsJsonAsync(TestConstants.Endpoints.CreateHousehold, new CreateHouseholdRequestDto(TestConstants.TestData.HouseholdName));

        var updateRequest = new UpdateMyPreferencesRequestDto(DietaryProfile.Vegetarian, new List<string> { "nuts" });
        var updateResponse = await client.PutAsJsonAsync(TestConstants.Endpoints.UpdateMyPreferences, updateRequest);

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updateBody = await GetFromJsonAsync<UpdateMyPreferencesResponseDto>(updateResponse);
        updateBody.ShouldNotBeNull();
        updateBody.DietaryProfile.ShouldBe(DietaryProfile.Vegetarian);
        updateBody.ExcludedIngredients.ShouldContain("nuts");

        var getResponse = await client.GetAsync(TestConstants.Endpoints.GetMyPreferences);
        getResponse.EnsureSuccessStatusCode();
        var getBody = await GetFromJsonAsync<GetMyPreferencesResponseDto>(getResponse);
        getBody.ShouldNotBeNull();
        getBody.DietaryProfile.ShouldBe(DietaryProfile.Vegetarian);
        getBody.ExcludedIngredients.ShouldContain("nuts");
    }

    [Fact]
    public async Task GetMyProfile_Returns200_WithDisplayName_WhenMemberHasHousehold()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);

        await client.PostAsJsonAsync(TestConstants.Endpoints.CreateHousehold, new CreateHouseholdRequestDto(TestConstants.TestData.HouseholdName));
        await client.PutAsJsonAsync(TestConstants.Endpoints.UpdateMyProfile, new UpdateMyProfileRequestDto("My Display Name", null));
        var response = await client.GetAsync(TestConstants.Endpoints.GetMyProfile);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await GetFromJsonAsync<GetMyProfileResponseDto>(response);
        body.ShouldNotBeNull();
        body.DisplayName.ShouldBe("My Display Name");
    }

    [Fact]
    public async Task GetMyProfile_Returns404_WhenUserHasNoHousehold()
    {
        await ResetAsync();
        var client = CreateClientWithToken(Guid.NewGuid());

        var response = await client.GetAsync(TestConstants.Endpoints.GetMyProfile);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateMyProfile_Returns200_AndPersists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);

        await client.PostAsJsonAsync(TestConstants.Endpoints.CreateHousehold, new CreateHouseholdRequestDto(TestConstants.TestData.HouseholdName));

        var updateRequest = new UpdateMyProfileRequestDto("My Display Name", "https://example.com/avatar.png");
        var updateResponse = await client.PutAsJsonAsync(TestConstants.Endpoints.UpdateMyProfile, updateRequest);

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updateBody = await GetFromJsonAsync<UpdateMyProfileResponseDto>(updateResponse);
        updateBody.ShouldNotBeNull();
        updateBody.DisplayName.ShouldBe("My Display Name");
        updateBody.AvatarUrl.ShouldBe("https://example.com/avatar.png");

        var getResponse = await client.GetAsync(TestConstants.Endpoints.GetMyProfile);
        getResponse.EnsureSuccessStatusCode();
        var getBody = await GetFromJsonAsync<GetMyProfileResponseDto>(getResponse);
        getBody.ShouldNotBeNull();
        getBody.DisplayName.ShouldBe("My Display Name");
    }

    [Fact]
    public async Task GetHouseholdMembersPreferences_Returns200_WithMembers_WhenOwner()
    {
        await ResetAsync();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ownerClient = CreateClientWithToken(ownerId);
        var memberClient = CreateClientWithToken(memberId, TestConstants.TestData.InviteeEmail);

        var createResponse = await ownerClient.PostAsJsonAsync(TestConstants.Endpoints.CreateHousehold, new CreateHouseholdRequestDto(TestConstants.TestData.HouseholdName));
        createResponse.EnsureSuccessStatusCode();
        var created = await GetFromJsonAsync<CreateHouseholdResponseDto>(createResponse);
        created.ShouldNotBeNull();

        var inviteResponse = await ownerClient.PostAsJsonAsync(TestConstants.Endpoints.SendInvite,
            new SendHouseholdInvitationRequestDto(TestConstants.TestData.InviteeEmail, created.Id.ToString()));
        inviteResponse.EnsureSuccessStatusCode();
        var inviteBody = await GetFromJsonAsync<SendHouseholdInvitationResponseDto>(inviteResponse);
        inviteBody.ShouldNotBeNull();
        var joinResponse = await memberClient.PostAsJsonAsync(TestConstants.Endpoints.Join, new AcceptHouseholdInvitationRequestDto(inviteBody.Code));
        joinResponse.EnsureSuccessStatusCode();

        var response = await ownerClient.GetAsync(TestConstants.Endpoints.GetHouseholdMembersPreferences);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await GetFromJsonAsync<GetHouseholdMembersPreferencesResponseDto>(response);
        body.ShouldNotBeNull();
        body.Members.ShouldNotBeNull();
        body.Members.Count.ShouldBeGreaterThanOrEqualTo(2);
    }
}
