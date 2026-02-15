using System.Net;
using System.Net.Http.Json;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.IntegrationTests.Constants;
using PantryCloud.Pantry.IntegrationTests.Infrastructure;
using PantryCloud.SharedKernel.Enums;
using Shouldly;

namespace PantryCloud.Pantry.IntegrationTests.Tests;

public class CreateAndListPantryItemsTests(PantryTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task CreatePantryItem_Returns401_WhenNoToken()
    {
        await ResetAsync();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(Guid.NewGuid(), householdId);
        var client = Fixture.CreateClient();

        var request = new CreatePantryItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece, null, TestConstants.TestData.Category, null, null);
        var response = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateItem, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePantryItem_Returns404_WhenUserHasNoHousehold()
    {
        await ResetAsync();
        var client = CreateClientWithToken(Guid.NewGuid());

        var request = new CreatePantryItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece, null, TestConstants.TestData.Category, null, null);
        var response = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateItem, request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePantryItem_Returns201_AndItem_WhenUserHasHousehold()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var request = new CreatePantryItemRequestDto(TestConstants.TestData.ItemName, 2.5m, Unit.Liter, null, TestConstants.TestData.Category, "Fresh", null);
        var response = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateItem, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await GetFromJsonAsync<CreatePantryItemResponseDto>(response);
        body.ShouldNotBeNull();
        body.Id.ShouldNotBe(Guid.Empty);
        body.HouseholdId.ShouldBe(householdId);
        body.Name.ShouldBe(TestConstants.TestData.ItemName);
        body.Quantity.ShouldBe(2.5m);
        body.Unit.ShouldBe(Unit.Liter);
        body.Category.ShouldBe(TestConstants.TestData.Category);
        body.Notes.ShouldBe("Fresh");
        body.CreatedBy.ShouldBe(userId);
        body.RowVersion.ShouldNotBeNull();
        body.RowVersion.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ListPantryItems_Returns401_WhenNoToken()
    {
        await ResetAsync();
        var client = Fixture.CreateClient();

        var response = await client.GetAsync(TestConstants.Endpoints.ListItems);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListPantryItems_Returns404_WhenUserHasNoHousehold()
    {
        await ResetAsync();
        var client = CreateClientWithToken(Guid.NewGuid());

        var response = await client.GetAsync(TestConstants.Endpoints.ListItems);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListPantryItems_Returns200_WithItems_WhenUserHasHousehold()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createRequest = new CreatePantryItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece, null, TestConstants.TestData.Category, null, null);
        var createResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateItem, createRequest);
        createResponse.EnsureSuccessStatusCode();

        var listResponse = await client.GetAsync(TestConstants.Endpoints.ListItems);

        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await GetFromJsonAsync<ListPantryItemsResponseDto>(listResponse);
        body.ShouldNotBeNull();
        body.Items.Count.ShouldBeGreaterThanOrEqualTo(1);
        body.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
        body.Items.ShouldContain(x => x.Name == TestConstants.TestData.ItemName && x.Category == TestConstants.TestData.Category);
    }

    [Fact]
    public async Task ListPantryItems_Returns200_FilteredByCategory_WhenQueryProvided()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        await client.PostAsJsonAsync(TestConstants.Endpoints.CreateItem, new CreatePantryItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece, null, TestConstants.TestData.Category, null, null));
        await client.PostAsJsonAsync(TestConstants.Endpoints.CreateItem, new CreatePantryItemRequestDto("Eggs", 1, Unit.Piece, null, "Protein", null, null));

        var listResponse = await client.GetAsync(TestConstants.Endpoints.ListItems + "?category=" + Uri.EscapeDataString(TestConstants.TestData.Category));

        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await GetFromJsonAsync<ListPantryItemsResponseDto>(listResponse);
        body.ShouldNotBeNull();
        body.Items.ShouldAllBe(x => x.Category == TestConstants.TestData.Category);
        body.Items.ShouldContain(x => x.Name == TestConstants.TestData.ItemName);
    }
}
