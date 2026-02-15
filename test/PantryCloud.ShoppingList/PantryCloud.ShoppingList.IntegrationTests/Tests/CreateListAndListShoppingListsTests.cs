using System.Net;
using System.Net.Http.Json;
using PantryCloud.ShoppingList.Application.Dtos;
using PantryCloud.ShoppingList.IntegrationTests.Constants;
using PantryCloud.ShoppingList.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.ShoppingList.IntegrationTests.Tests;

public class CreateListAndListShoppingListsTests(ShoppingListTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task CreateShoppingList_Returns401_WhenNoToken()
    {
        await ResetAsync();
        var client = Fixture.CreateClient();

        var response = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateShoppingList_Returns404_WhenUserHasNoHousehold()
    {
        await ResetAsync();
        var client = CreateClientWithToken(Guid.NewGuid());

        var response = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateShoppingList_Returns201_AndList_WhenUserHasHousehold()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var response = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await GetFromJsonAsync<CreateShoppingListResponseDto>(response);
        body.ShouldNotBeNull();
        body.Id.ShouldNotBe(Guid.Empty);
        body.HouseholdId.ShouldBe(householdId);
        body.Name.ShouldBe(TestConstants.TestData.ListName);
        body.CreatedBy.ShouldBe(userId);
    }

    [Fact]
    public async Task CreateShoppingList_Returns409_WhenDuplicateNameInSameHousehold()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var firstResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));
        firstResponse.EnsureSuccessStatusCode();

        var secondResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ListShoppingLists_Returns401_WhenNoToken()
    {
        await ResetAsync();
        var client = Fixture.CreateClient();

        var response = await client.GetAsync(TestConstants.Endpoints.ListLists);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListShoppingLists_Returns404_WhenUserHasNoHousehold()
    {
        await ResetAsync();
        var client = CreateClientWithToken(Guid.NewGuid());

        var response = await client.GetAsync(TestConstants.Endpoints.ListLists);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListShoppingLists_Returns200_WithLists_WhenUserHasHousehold()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));

        var listResponse = await client.GetAsync(TestConstants.Endpoints.ListLists);

        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await GetFromJsonAsync<ListShoppingListsResponseDto>(listResponse);
        body.ShouldNotBeNull();
        body.Lists.Count.ShouldBeGreaterThanOrEqualTo(1);
        body.Lists.ShouldContain(x => x.Name == TestConstants.TestData.ListName);
    }

    [Fact]
    public async Task GetShoppingList_Returns404_WhenListDoesNotExist()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var response = await client.GetAsync(TestConstants.Endpoints.GetList(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetShoppingList_Returns200_WithListAndItems_WhenListExists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));
        createResponse.EnsureSuccessStatusCode();
        var created = await GetFromJsonAsync<CreateShoppingListResponseDto>(createResponse);
        created.ShouldNotBeNull();

        var getResponse = await client.GetAsync(TestConstants.Endpoints.GetList(created.Id));

        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await GetFromJsonAsync<GetShoppingListResponseDto>(getResponse);
        body.ShouldNotBeNull();
        body.Id.ShouldBe(created.Id);
        body.Name.ShouldBe(TestConstants.TestData.ListName);
        body.Items.ShouldNotBeNull();
        body.Items.Count.ShouldBe(0);
    }

    [Fact]
    public async Task DeleteShoppingList_Returns200_WhenListExists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));
        createResponse.EnsureSuccessStatusCode();
        var created = await GetFromJsonAsync<CreateShoppingListResponseDto>(createResponse);
        created.ShouldNotBeNull();

        var deleteResponse = await client.DeleteAsync(TestConstants.Endpoints.DeleteList(created.Id));

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deleteBody = await GetFromJsonAsync<DeleteShoppingListResponseDto>(deleteResponse);
        deleteBody.ShouldNotBeNull();
        deleteBody.Id.ShouldBe(created.Id);

        var getResponse = await client.GetAsync(TestConstants.Endpoints.GetList(created.Id));
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteShoppingList_Returns404_WhenListDoesNotExist()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var response = await client.DeleteAsync(TestConstants.Endpoints.DeleteList(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
