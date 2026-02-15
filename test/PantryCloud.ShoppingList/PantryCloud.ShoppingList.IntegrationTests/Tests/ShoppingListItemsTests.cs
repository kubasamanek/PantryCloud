using System.Net;
using System.Net.Http.Json;
using PantryCloud.ShoppingList.Application.Dtos;
using PantryCloud.ShoppingList.IntegrationTests.Constants;
using PantryCloud.ShoppingList.IntegrationTests.Infrastructure;
using PantryCloud.SharedKernel.Enums;
using PantryCloud.ShoppingList.Core.Enums;
using Shouldly;

namespace PantryCloud.ShoppingList.IntegrationTests.Tests;

public class ShoppingListItemsTests(ShoppingListTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task AddShoppingListItem_Returns201_AndItem_WhenListExists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createListResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));
        createListResponse.EnsureSuccessStatusCode();
        var list = await GetFromJsonAsync<CreateShoppingListResponseDto>(createListResponse);
        list.ShouldNotBeNull();

        var addRequest = new AddShoppingListItemRequestDto(TestConstants.TestData.ItemName, 2, Unit.Piece, ItemSource.Manual);
        var addResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.AddItem(list.Id), addRequest);

        addResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await GetFromJsonAsync<AddShoppingListItemResponseDto>(addResponse);
        body.ShouldNotBeNull();
        body.ShoppingListId.ShouldBe(list.Id);
        body.Name.ShouldBe(TestConstants.TestData.ItemName);
        body.Quantity.ShouldBe(2);
        body.Unit.ShouldBe(Unit.Piece);
        body.IsChecked.ShouldBeFalse();
        body.Source.ShouldBe(ItemSource.Manual);
        body.CreatedBy.ShouldBe(userId);
    }

    [Fact]
    public async Task AddShoppingListItem_Returns404_WhenListDoesNotExist()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var addRequest = new AddShoppingListItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece);
        var response = await client.PostAsJsonAsync(TestConstants.Endpoints.AddItem(Guid.NewGuid()), addRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddShoppingListItemsBatch_Returns201_WithItems_WhenListExists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createListResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));
        createListResponse.EnsureSuccessStatusCode();
        var list = await GetFromJsonAsync<CreateShoppingListResponseDto>(createListResponse);
        list.ShouldNotBeNull();

        var batchRequest = new AddShoppingListItemsBatchRequestDto(new[]
        {
            new AddShoppingListItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece),
            new AddShoppingListItemRequestDto(TestConstants.TestData.ItemName2, 1, Unit.Piece)
        });
        var batchResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.AddItemsBatch(list.Id), batchRequest);

        batchResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await GetFromJsonAsync<AddShoppingListItemsBatchResponseDto>(batchResponse);
        body.ShouldNotBeNull();
        body.Items.Count.ShouldBe(2);
        body.Items.ShouldContain(x => x.Name == TestConstants.TestData.ItemName);
        body.Items.ShouldContain(x => x.Name == TestConstants.TestData.ItemName2);
    }

    [Fact]
    public async Task UpdateShoppingListItem_Returns200_AndPersists_WhenItemExists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createListResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));
        createListResponse.EnsureSuccessStatusCode();
        var list = await GetFromJsonAsync<CreateShoppingListResponseDto>(createListResponse);
        list.ShouldNotBeNull();

        var addResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.AddItem(list.Id), new AddShoppingListItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece));
        addResponse.EnsureSuccessStatusCode();
        var item = await GetFromJsonAsync<AddShoppingListItemResponseDto>(addResponse);
        item.ShouldNotBeNull();

        var updateRequest = new UpdateShoppingListItemRequestDto("Updated Milk", 3, Unit.Liter, item.RowVersion);
        var updateResponse = await client.PutAsJsonAsync(TestConstants.Endpoints.UpdateItem(list.Id, item.Id), updateRequest);

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updateBody = await GetFromJsonAsync<UpdateShoppingListItemResponseDto>(updateResponse);
        updateBody.ShouldNotBeNull();
        updateBody.Name.ShouldBe("Updated Milk");
        updateBody.Quantity.ShouldBe(3);
        updateBody.Unit.ShouldBe(Unit.Liter);

        var getListResponse = await client.GetAsync(TestConstants.Endpoints.GetList(list.Id));
        getListResponse.EnsureSuccessStatusCode();
        var getBody = await GetFromJsonAsync<GetShoppingListResponseDto>(getListResponse);
        getBody.ShouldNotBeNull();
        getBody.Items.ShouldContain(x => x.Id == item.Id && x.Name == "Updated Milk");
    }

    [Fact]
    public async Task CheckShoppingListItem_Returns200_AndTogglesChecked_WhenItemExists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createListResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));
        createListResponse.EnsureSuccessStatusCode();
        var list = await GetFromJsonAsync<CreateShoppingListResponseDto>(createListResponse);
        list.ShouldNotBeNull();

        var addResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.AddItem(list.Id), new AddShoppingListItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece));
        addResponse.EnsureSuccessStatusCode();
        var item = await GetFromJsonAsync<AddShoppingListItemResponseDto>(addResponse);
        item.ShouldNotBeNull();
        item.IsChecked.ShouldBeFalse();

        var checkResponse = await client.PatchAsync(TestConstants.Endpoints.CheckItem(list.Id, item.Id), null);

        checkResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var checkBody = await GetFromJsonAsync<CheckShoppingListItemResponseDto>(checkResponse);
        checkBody.ShouldNotBeNull();
        checkBody.IsChecked.ShouldBeTrue();
        checkBody.CheckedBy.ShouldBe(userId);

        var getListResponse = await client.GetAsync(TestConstants.Endpoints.GetList(list.Id));
        getListResponse.EnsureSuccessStatusCode();
        var getBody = await GetFromJsonAsync<GetShoppingListResponseDto>(getListResponse);
        getBody.ShouldNotBeNull();
        getBody.Items.ShouldContain(x => x.Id == item.Id && x.IsChecked);
    }

    [Fact]
    public async Task DeleteShoppingListItem_Returns200_WhenItemExists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createListResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));
        createListResponse.EnsureSuccessStatusCode();
        var list = await GetFromJsonAsync<CreateShoppingListResponseDto>(createListResponse);
        list.ShouldNotBeNull();

        var addResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.AddItem(list.Id), new AddShoppingListItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece));
        addResponse.EnsureSuccessStatusCode();
        var item = await GetFromJsonAsync<AddShoppingListItemResponseDto>(addResponse);
        item.ShouldNotBeNull();

        var deleteResponse = await client.DeleteAsync(TestConstants.Endpoints.DeleteItem(list.Id, item.Id));

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deleteBody = await GetFromJsonAsync<DeleteShoppingListItemResponseDto>(deleteResponse);
        deleteBody.ShouldNotBeNull();
        deleteBody.ItemId.ShouldBe(item.Id);

        var getListResponse = await client.GetAsync(TestConstants.Endpoints.GetList(list.Id));
        getListResponse.EnsureSuccessStatusCode();
        var getBody = await GetFromJsonAsync<GetShoppingListResponseDto>(getListResponse);
        getBody.ShouldNotBeNull();
        getBody.Items.ShouldNotContain(x => x.Id == item.Id);
    }

    [Fact]
    public async Task DeleteShoppingListItem_Returns404_WhenItemDoesNotExist()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createListResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateList, new CreateShoppingListRequestDto(TestConstants.TestData.ListName));
        createListResponse.EnsureSuccessStatusCode();
        var list = await GetFromJsonAsync<CreateShoppingListResponseDto>(createListResponse);
        list.ShouldNotBeNull();

        var response = await client.DeleteAsync(TestConstants.Endpoints.DeleteItem(list.Id, Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
