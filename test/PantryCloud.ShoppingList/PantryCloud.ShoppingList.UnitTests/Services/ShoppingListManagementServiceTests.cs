using PantryCloud.SharedKernel.Enums;
using PantryCloud.ShoppingList.Application.Dtos;
using PantryCloud.ShoppingList.Core.Entities;
using PantryCloud.ShoppingList.Core.Enums;
using PantryCloud.ShoppingList.Infrastructure.Persistence;
using PantryCloud.ShoppingList.Infrastructure.Services;
using Shouldly;

namespace PantryCloud.ShoppingList.UnitTests.Services;

public class ShoppingListManagementServiceTests
{
    [Fact]
    public async Task CreateShoppingListAsync_ShouldSucceed_WhenUserInHousehold()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(CreateShoppingListAsync_ShouldSucceed_WhenUserInHousehold), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.CreateShoppingListAsync(new CreateShoppingListRequestDto(Constants.ShoppingList.Name), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.Name.ShouldBe(Constants.ShoppingList.Name);
        result.Value.HouseholdId.ShouldBe(Constants.Household.Id);
        result.Value.CreatedBy.ShouldBe(Constants.User.Id);
    }

    [Fact]
    public async Task CreateShoppingListAsync_ShouldReturnDuplicateName_WhenListWithSameNameExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(CreateShoppingListAsync_ShouldReturnDuplicateName_WhenListWithSameNameExists), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        await SeedShoppingListAsync(db, Constants.Household.Id, Constants.ShoppingList.DuplicateName);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.CreateShoppingListAsync(new CreateShoppingListRequestDto(Constants.ShoppingList.DuplicateName), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(Constants.Errors.DuplicateListName);
    }

    [Fact]
    public async Task CreateShoppingListAsync_ShouldReturnHouseholdNotFound_WhenUserNotInHousehold()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(CreateShoppingListAsync_ShouldReturnHouseholdNotFound_WhenUserNotInHousehold), userContext);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.CreateShoppingListAsync(new CreateShoppingListRequestDto(Constants.ShoppingList.Name), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(Constants.Errors.HouseholdNotFound);
    }

    [Fact]
    public async Task DeleteShoppingListAsync_ShouldSucceed_WhenListExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(DeleteShoppingListAsync_ShouldSucceed_WhenListExists), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var list = await SeedShoppingListAsync(db, Constants.Household.Id, Constants.ShoppingList.Name);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.DeleteShoppingListAsync(new DeleteShoppingListRequestDto(list.Id), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(list.Id);
        var deleted = await db.ShoppingLists.FindAsync(list.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteShoppingListAsync_ShouldReturnNotFound_WhenListDoesNotExist()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(DeleteShoppingListAsync_ShouldReturnNotFound_WhenListDoesNotExist), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.DeleteShoppingListAsync(new DeleteShoppingListRequestDto(Constants.NonExistentListId), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(Constants.Errors.ShoppingListNotFound);
    }

    [Fact]
    public async Task GetShoppingListAsync_ShouldSucceed_WhenListExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetShoppingListAsync_ShouldSucceed_WhenListExists), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var list = await SeedShoppingListAsync(db, Constants.Household.Id, Constants.ShoppingList.Name);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.GetShoppingListAsync(new GetShoppingListRequestDto(list.Id), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(list.Id);
        result.Value.Name.ShouldBe(Constants.ShoppingList.Name);
        result.Value.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetShoppingListAsync_ShouldReturnNotFound_WhenListDoesNotExist()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetShoppingListAsync_ShouldReturnNotFound_WhenListDoesNotExist), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.GetShoppingListAsync(new GetShoppingListRequestDto(Constants.NonExistentListId), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(Constants.Errors.ShoppingListNotFound);
    }

    [Fact]
    public async Task ListShoppingListsAsync_ShouldReturnLists_WhenListsExist()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(ListShoppingListsAsync_ShouldReturnLists_WhenListsExist), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        await SeedShoppingListAsync(db, Constants.Household.Id, "List 1");
        await SeedShoppingListAsync(db, Constants.Household.Id, "List 2");
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.ListShoppingListsAsync(new ListShoppingListsRequestDto(Constants.Pagination.Page1, Constants.Pagination.PageSize50), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Lists.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task ListShoppingListsAsync_ShouldReturnEmpty_WhenNoLists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(ListShoppingListsAsync_ShouldReturnEmpty_WhenNoLists), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.ListShoppingListsAsync(new ListShoppingListsRequestDto(Constants.Pagination.Page1, Constants.Pagination.PageSize50), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Lists.ShouldBeEmpty();
        result.Value.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task AddShoppingListItemAsync_ShouldSucceed_WhenListExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(AddShoppingListItemAsync_ShouldSucceed_WhenListExists), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var list = await SeedShoppingListAsync(db, Constants.Household.Id, Constants.ShoppingList.Name);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());
        var request = new AddShoppingListItemRequestDto(Constants.ShoppingListItem.Name, Constants.ShoppingListItem.Quantity, Constants.ShoppingListItem.Unit);

        var result = await sut.AddShoppingListItemAsync(list.Id, request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Name.ShouldBe(Constants.ShoppingListItem.Name);
        result.Value.Quantity.ShouldBe(Constants.ShoppingListItem.Quantity);
        result.Value.RowVersion.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddShoppingListItemAsync_ShouldReturnUnauthorized_WhenListNotFound()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(AddShoppingListItemAsync_ShouldReturnUnauthorized_WhenListNotFound), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());
        var request = new AddShoppingListItemRequestDto(Constants.ShoppingListItem.Name, Constants.ShoppingListItem.Quantity, Constants.ShoppingListItem.Unit);

        var result = await sut.AddShoppingListItemAsync(Constants.NonExistentListId, request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(Constants.Errors.ShoppingListNotFound);
    }

    [Fact]
    public async Task AddShoppingListItemsBatchAsync_ShouldSucceed_WhenListExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(AddShoppingListItemsBatchAsync_ShouldSucceed_WhenListExists), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var list = await SeedShoppingListAsync(db, Constants.Household.Id, Constants.ShoppingList.Name);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());
        var items = new List<AddShoppingListItemRequestDto>
        {
            new("Milk", 1m, Unit.Liter),
            new("Bread", 2m, Unit.Piece)
        };
        var request = new AddShoppingListItemsBatchRequestDto(items);

        var result = await sut.AddShoppingListItemsBatchAsync(list.Id, request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task UpdateShoppingListItemAsync_ShouldSucceed_WhenItemExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdateShoppingListItemAsync_ShouldSucceed_WhenItemExists), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var (list, item) = await SeedShoppingListWithItemAsync(db, Constants.Household.Id);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());
        var request = new UpdateShoppingListItemRequestDto("Updated Milk", 3m, Unit.Liter, item.RowVersion);

        var result = await sut.UpdateShoppingListItemAsync(list.Id, item.Id, request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Name.ShouldBe("Updated Milk");
        result.Value.Quantity.ShouldBe(3m);
    }

    [Fact]
    public async Task UpdateShoppingListItemAsync_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdateShoppingListItemAsync_ShouldReturnNotFound_WhenItemDoesNotExist), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var list = await SeedShoppingListAsync(db, Constants.Household.Id, Constants.ShoppingList.Name);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());
        var request = new UpdateShoppingListItemRequestDto("Milk", 2m, Unit.Liter, Constants.RowVersion.Default);

        var result = await sut.UpdateShoppingListItemAsync(list.Id, Constants.NonExistentItemId, request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(Constants.Errors.ItemNotFound);
    }

    [Fact]
    public async Task DeleteShoppingListItemAsync_ShouldSucceed_WhenItemExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(DeleteShoppingListItemAsync_ShouldSucceed_WhenItemExists), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var (list, item) = await SeedShoppingListWithItemAsync(db, Constants.Household.Id);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.DeleteShoppingListItemAsync(new DeleteShoppingListItemRequestDto(list.Id, item.Id), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        var deleted = await db.ShoppingListItems.FindAsync(item.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteShoppingListItemAsync_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(DeleteShoppingListItemAsync_ShouldReturnNotFound_WhenItemDoesNotExist), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var list = await SeedShoppingListAsync(db, Constants.Household.Id, Constants.ShoppingList.Name);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.DeleteShoppingListItemAsync(new DeleteShoppingListItemRequestDto(list.Id, Constants.NonExistentItemId), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(Constants.Errors.ItemNotFound);
    }

    [Fact]
    public async Task CheckShoppingListItemAsync_ShouldToggleCheckStatus()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(CheckShoppingListItemAsync_ShouldToggleCheckStatus), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var (list, item) = await SeedShoppingListWithItemAsync(db, Constants.Household.Id);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.CheckShoppingListItemAsync(new CheckShoppingListItemRequestDto(list.Id, item.Id), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.IsChecked.ShouldBeTrue();
        result.Value.CheckedBy.ShouldBe(Constants.User.Id);
        result.Value.AllItemsChecked.ShouldBeTrue();
    }

    [Fact]
    public async Task CheckShoppingListItemAsync_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(CheckShoppingListItemAsync_ShouldReturnNotFound_WhenItemDoesNotExist), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var list = await SeedShoppingListAsync(db, Constants.Household.Id, Constants.ShoppingList.Name);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        var result = await sut.CheckShoppingListItemAsync(new CheckShoppingListItemRequestDto(list.Id, Constants.NonExistentItemId), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(Constants.Errors.ItemNotFound);
    }

    [Fact]
    public async Task CheckShoppingListItemAsync_ShouldSetAllItemsChecked_WhenAllItemsChecked()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(CheckShoppingListItemAsync_ShouldSetAllItemsChecked_WhenAllItemsChecked), userContext);
        await SeedMembershipAsync(db, Constants.User.Id, Constants.Household.Id);
        var (list, item1) = await SeedShoppingListWithItemAsync(db, Constants.Household.Id);
        var item2 = await SeedItemAsync(db, list.Id, "Bread", 1m, Unit.Piece);
        var sut = new ShoppingListManagementService(db, userContext, TestHelper.MockLogger<ShoppingListManagementService>());

        await sut.CheckShoppingListItemAsync(new CheckShoppingListItemRequestDto(list.Id, item1.Id), CancellationToken.None);
        var result = await sut.CheckShoppingListItemAsync(new CheckShoppingListItemRequestDto(list.Id, item2.Id), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.AllItemsChecked.ShouldBeTrue();
        result.Value.HouseholdId.ShouldBe(Constants.Household.Id);
        result.Value.ShoppingListId.ShouldBe(list.Id);
    }

    private static async Task SeedMembershipAsync(ShoppingListDbContext db, Guid userId, Guid householdId)
    {
        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = userId,
            HouseholdId = householdId,
            JoinedAt = Constants.Dates.ThirtyDaysAgo
        });
        await db.SaveChangesAsync();
    }

    private static async Task<Core.Entities.ShoppingList> SeedShoppingListAsync(ShoppingListDbContext db, Guid householdId, string name)
    {
        var list = new Core.Entities.ShoppingList
        {
            HouseholdId = householdId,
            Name = name
        };
        db.ShoppingLists.Add(list);
        await db.SaveChangesAsync();
        return list;
    }

    private static async Task<(Core.Entities.ShoppingList List, ShoppingListItem Item)> SeedShoppingListWithItemAsync(ShoppingListDbContext db, Guid householdId)
    {
        var list = new Core.Entities.ShoppingList { HouseholdId = householdId, Name = Constants.ShoppingList.Name };
        db.ShoppingLists.Add(list);
        await db.SaveChangesAsync();

        var item = new ShoppingListItem
        {
            ShoppingListId = list.Id,
            Name = Constants.ShoppingListItem.Name,
            Quantity = Constants.ShoppingListItem.Quantity,
            Unit = Constants.ShoppingListItem.Unit,
            Source = ItemSource.Manual,
            RowVersion = Constants.RowVersion.Default
        };
        db.ShoppingListItems.Add(item);
        await db.SaveChangesAsync();
        return (list, item);
    }

    private static async Task<ShoppingListItem> SeedItemAsync(ShoppingListDbContext db, Guid listId, string name, decimal quantity, Unit unit)
    {
        var item = new ShoppingListItem
        {
            ShoppingListId = listId,
            Name = name,
            Quantity = quantity,
            Unit = unit,
            Source = ItemSource.Manual,
            RowVersion = Constants.RowVersion.Default
        };
        db.ShoppingListItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }
}
