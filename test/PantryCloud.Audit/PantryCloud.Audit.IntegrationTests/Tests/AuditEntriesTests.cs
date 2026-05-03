using System.Net;
using PantryCloud.Audit.Application.Dtos;
using PantryCloud.Audit.IntegrationTests.Constants;
using PantryCloud.Audit.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Audit.IntegrationTests.Tests;

public class AuditEntriesTests(AuditTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task ListHouseholdAuditEntries_Returns401_WhenNoToken()
    {
        await ResetAsync();
        var householdId = Guid.NewGuid();
        var client = Fixture.CreateClient();

        var response = await client.GetAsync(TestConstants.Endpoints.ListHouseholdEntries(householdId));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListHouseholdAuditEntries_Returns403_WhenUserNotInHousehold()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, Guid.NewGuid());
        var client = CreateClientWithToken(userId);

        var response = await client.GetAsync(TestConstants.Endpoints.ListHouseholdEntries(householdId));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListHouseholdAuditEntries_Returns200_WithEmptyEntries_WhenMemberHasNoAuditData()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var response = await client.GetAsync(TestConstants.Endpoints.ListHouseholdEntries(householdId));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await GetFromJsonAsync<ListAuditEntriesResponseDto>(response);
        body.ShouldNotBeNull();
        body.Entries.ShouldNotBeNull();
        body.Entries.Count.ShouldBe(0);
        body.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task ListHouseholdAuditEntries_Returns200_WithQueryParams_WhenMember()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var url = TestConstants.Endpoints.ListHouseholdEntries(householdId) + "?page=1&pageSize=10";
        var response = await client.GetAsync(url);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await GetFromJsonAsync<ListAuditEntriesResponseDto>(response);
        body.ShouldNotBeNull();
        body.Page.ShouldBe(1);
        body.PageSize.ShouldBe(10);
    }
}
