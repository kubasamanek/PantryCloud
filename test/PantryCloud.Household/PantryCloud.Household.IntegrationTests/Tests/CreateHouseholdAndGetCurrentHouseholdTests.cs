using System.Net;
using System.Net.Http.Json;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.IntegrationTests.Constants;
using PantryCloud.Household.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Household.IntegrationTests.Tests;

public class CreateHouseholdAndGetCurrentHouseholdTests(HouseholdTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task CreateHousehold_Returns201_AndHousehold_WhenUserAuthenticated()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);

        var request = new CreateHouseholdRequestDto(TestConstants.TestData.HouseholdName);
        var response = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateHousehold, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await GetFromJsonAsync<CreateHouseholdResponseDto>(response);
        body.ShouldNotBeNull();
        body.Name.ShouldBe(TestConstants.TestData.HouseholdName);
        body.OwnerId.ShouldBe(userId);
        body.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetCurrentHousehold_Returns404_WhenUserHasNoHousehold()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);

        var response = await client.GetAsync(TestConstants.Endpoints.GetCurrentHousehold);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCurrentHousehold_Returns200_WithHousehold_AfterCreate()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);

        var createRequest = new CreateHouseholdRequestDto(TestConstants.TestData.HouseholdName);
        var createResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateHousehold, createRequest);
        createResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync(TestConstants.Endpoints.GetCurrentHousehold);

        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await GetFromJsonAsync<GetCurrentHouseholdResponseDto>(getResponse);
        body.ShouldNotBeNull();
        body.Name.ShouldBe(TestConstants.TestData.HouseholdName);
        body.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetCurrentHousehold_Returns401_WhenNoBearerToken()
    {
        await ResetAsync();
        var client = Fixture.CreateClient();

        var response = await client.GetAsync(TestConstants.Endpoints.GetCurrentHousehold);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
