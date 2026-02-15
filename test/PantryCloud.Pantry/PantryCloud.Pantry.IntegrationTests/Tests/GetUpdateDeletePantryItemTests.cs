using System.Net;
using System.Net.Http.Json;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.IntegrationTests.Constants;
using PantryCloud.Pantry.IntegrationTests.Infrastructure;
using PantryCloud.SharedKernel.Enums;
using Shouldly;

namespace PantryCloud.Pantry.IntegrationTests.Tests;

public class GetUpdateDeletePantryItemTests(PantryTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task GetPantryItem_Returns401_WhenNoToken()
    {
        await ResetAsync();
        var client = Fixture.CreateClient();

        var response = await client.GetAsync(TestConstants.Endpoints.GetItem(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPantryItem_Returns404_WhenUserHasNoHousehold()
    {
        await ResetAsync();
        var client = CreateClientWithToken(Guid.NewGuid());

        var response = await client.GetAsync(TestConstants.Endpoints.GetItem(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPantryItem_Returns404_WhenItemDoesNotExist()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var response = await client.GetAsync(TestConstants.Endpoints.GetItem(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPantryItem_Returns200_WithItem_WhenItemExists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateItem,
            new CreatePantryItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece, null, TestConstants.TestData.Category, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await GetFromJsonAsync<CreatePantryItemResponseDto>(createResponse);
        created.ShouldNotBeNull();

        var getResponse = await client.GetAsync(TestConstants.Endpoints.GetItem(created.Id));

        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await GetFromJsonAsync<GetPantryItemResponseDto>(getResponse);
        body.ShouldNotBeNull();
        body.Id.ShouldBe(created.Id);
        body.Name.ShouldBe(TestConstants.TestData.ItemName);
        body.HouseholdId.ShouldBe(householdId);
    }

    [Fact]
    public async Task UpdatePantryItem_Returns200_AndPersists_WhenItemExists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateItem,
            new CreatePantryItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece, null, TestConstants.TestData.Category, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await GetFromJsonAsync<CreatePantryItemResponseDto>(createResponse);
        created.ShouldNotBeNull();

        var updateRequest = new UpdatePantryItemRequestDto("Updated Milk", 3, Unit.Liter, DateTime.UtcNow.AddDays(7), "Dairy", "Updated notes", null, created.RowVersion);
        var updateResponse = await client.PutAsJsonAsync(TestConstants.Endpoints.UpdateItem(created.Id), updateRequest);

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updateBody = await GetFromJsonAsync<UpdatePantryItemResponseDto>(updateResponse);
        updateBody.ShouldNotBeNull();
        updateBody.Name.ShouldBe("Updated Milk");
        updateBody.Quantity.ShouldBe(3);
        updateBody.Unit.ShouldBe(Unit.Liter);
        updateBody.ModifiedBy.ShouldBe(userId);

        var getResponse = await client.GetAsync(TestConstants.Endpoints.GetItem(created.Id));
        getResponse.EnsureSuccessStatusCode();
        var getBody = await GetFromJsonAsync<GetPantryItemResponseDto>(getResponse);
        getBody.ShouldNotBeNull();
        getBody.Name.ShouldBe("Updated Milk");
        getBody.Quantity.ShouldBe(3);
    }

    [Fact]
    public async Task UpdatePantryItem_Returns404_WhenItemDoesNotExist()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var updateRequest = new UpdatePantryItemRequestDto("Name", 1, Unit.Piece, null, null, null, null, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        var response = await client.PutAsJsonAsync(TestConstants.Endpoints.UpdateItem(Guid.NewGuid()), updateRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePantryItem_Returns200_WhenItemExists()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var createResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateItem,
            new CreatePantryItemRequestDto(TestConstants.TestData.ItemName, 1, Unit.Piece, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await GetFromJsonAsync<CreatePantryItemResponseDto>(createResponse);
        created.ShouldNotBeNull();

        var deleteResponse = await client.DeleteAsync(TestConstants.Endpoints.DeleteItem(created.Id));

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deleteBody = await GetFromJsonAsync<DeletePantryItemResponseDto>(deleteResponse);
        deleteBody.ShouldNotBeNull();
        deleteBody.Id.ShouldBe(created.Id);
        deleteBody.InitiatedByUserId.ShouldBe(userId);

        var getResponse = await client.GetAsync(TestConstants.Endpoints.GetItem(created.Id));
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePantryItem_Returns404_WhenItemDoesNotExist()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        await SeedHouseholdMembershipAsync(userId, householdId);
        var client = CreateClientWithToken(userId);

        var response = await client.DeleteAsync(TestConstants.Endpoints.DeleteItem(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
