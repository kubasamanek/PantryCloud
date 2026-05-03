using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Core.Errors;
using PantryCloud.Pantry.Infrastructure.Services;
using Shouldly;

namespace PantryCloud.Pantry.UnitTests.Services;

public class PantryManagementServiceTests
{
    private readonly ILogger<PantryManagementService> _logger = TestHelper.MockLogger<PantryManagementService>();

    [Fact]
    public async Task CreatePantryItemAsync_ShouldCreateItem_WhenUserHasHousehold()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(CreatePantryItemAsync_ShouldCreateItem_WhenUserHasHousehold), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        var request = new CreatePantryItemRequestDto(
            Constants.PantryItem.Name,
            Constants.PantryItem.Quantity,
            Constants.PantryItem.Unit,
            Constants.PantryItem.ExpirationDate,
            Constants.PantryItem.Category,
            Constants.PantryItem.Notes,
            Constants.PantryItem.ImageUrl);

        var result = await service.CreatePantryItemAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Name.ShouldBe(Constants.PantryItem.Name);
        result.Value.Quantity.ShouldBe(Constants.PantryItem.Quantity);
        result.Value.Unit.ShouldBe(Constants.PantryItem.Unit);
        result.Value.HouseholdId.ShouldBe(Constants.Household.Id);
        result.Value.CreatedBy.ShouldBe(Constants.User.Id);

        var item = await db.PantryItems.FirstOrDefaultAsync(p => p.Id == result.Value.Id);
        item.ShouldNotBeNull();
        item.Name.ShouldBe(Constants.PantryItem.Name);
    }

    [Fact]
    public async Task CreatePantryItemAsync_ShouldReturnError_WhenUserNotInHousehold()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(CreatePantryItemAsync_ShouldReturnError_WhenUserNotInHousehold), userContext);

        var service = new PantryManagementService(db, userContext, _logger);

        var request = new CreatePantryItemRequestDto(
            Constants.PantryItem.Name,
            Constants.PantryItem.Quantity,
            Constants.PantryItem.Unit,
            null,
            null,
            null,
            null);

        var result = await service.CreatePantryItemAsync(request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(PantryErrors.HouseholdNotFound);
    }

    [Fact]
    public async Task GetPantryItemAsync_ShouldReturnItem_WhenItemExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetPantryItemAsync_ShouldReturnItem_WhenItemExists), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });

        var pantryItem = new PantryItem
        {
            Id = Constants.PantryItem.Id,
            HouseholdId = Constants.Household.Id,
            Name = Constants.PantryItem.Name,
            Quantity = Constants.PantryItem.Quantity,
            Unit = Constants.PantryItem.Unit,
            ExpirationDate = Constants.PantryItem.ExpirationDate,
            Category = Constants.PantryItem.Category,
            Notes = Constants.PantryItem.Notes,
            ImageUrl = Constants.PantryItem.ImageUrl,
            CreatedBy = Constants.User.Id,
            CreatedAt = DateTime.UtcNow,
            RowVersion = Constants.RowVersion.Default
        };

        db.PantryItems.Add(pantryItem);
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        var request = new GetPantryItemRequestDto(Constants.PantryItem.Id);
        var result = await service.GetPantryItemAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(Constants.PantryItem.Id);
        result.Value.Name.ShouldBe(Constants.PantryItem.Name);
        result.Value.Quantity.ShouldBe(Constants.PantryItem.Quantity);
    }

    [Fact]
    public async Task GetPantryItemAsync_ShouldReturnError_WhenItemNotFound()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetPantryItemAsync_ShouldReturnError_WhenItemNotFound), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        var nonExistentItemId = Constants.NonExistentItemId;
        var request = new GetPantryItemRequestDto(nonExistentItemId);
        var result = await service.GetPantryItemAsync(request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(PantryErrors.PantryItemNotFound);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_ShouldUpdateItem_WhenItemExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdatePantryItemAsync_ShouldUpdateItem_WhenItemExists), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });

        var pantryItem = new PantryItem
        {
            Id = Constants.PantryItem.Id,
            HouseholdId = Constants.Household.Id,
            Name = Constants.PantryItem.Name,
            Quantity = Constants.PantryItem.Quantity,
            Unit = Constants.PantryItem.Unit,
            CreatedBy = Constants.User.Id,
            CreatedAt = DateTime.UtcNow,
            RowVersion = Constants.RowVersion.Default
        };

        db.PantryItems.Add(pantryItem);
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        var request = new UpdatePantryItemRequestDto(
            Constants.UpdatedPantryItem.Name,
            Constants.UpdatedPantryItem.Quantity,
            Constants.UpdatedPantryItem.Unit,
            Constants.UpdatedPantryItem.ExpirationDate,
            Constants.UpdatedPantryItem.Category,
            Constants.UpdatedPantryItem.Notes,
            Constants.UpdatedPantryItem.ImageUrl,
            Constants.RowVersion.Default);

        var result = await service.UpdatePantryItemAsync(Constants.PantryItem.Id, request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Name.ShouldBe(Constants.UpdatedPantryItem.Name);
        result.Value.Quantity.ShouldBe(Constants.UpdatedPantryItem.Quantity);
        result.Value.Unit.ShouldBe(Constants.UpdatedPantryItem.Unit);
        result.Value.ModifiedBy.ShouldBe(Constants.User.Id);
        result.Value.ModifiedAt.ShouldNotBeNull();

        var updatedItem = await db.PantryItems.FirstOrDefaultAsync(p => p.Id == Constants.PantryItem.Id);
        updatedItem.ShouldNotBeNull();
        updatedItem.Name.ShouldBe(Constants.UpdatedPantryItem.Name);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_ShouldReturnError_WhenItemNotFound()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdatePantryItemAsync_ShouldReturnError_WhenItemNotFound), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        var nonExistentItemId = Constants.NonExistentItemId;
        var request = new UpdatePantryItemRequestDto(
            Constants.UpdatedPantryItem.Name,
            Constants.UpdatedPantryItem.Quantity,
            Constants.UpdatedPantryItem.Unit,
            null,
            null,
            null,
            null,
            Constants.RowVersion.Default);

        var result = await service.UpdatePantryItemAsync(nonExistentItemId, request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(PantryErrors.PantryItemNotFound);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_ShouldReturnError_WhenConcurrencyConflict()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdatePantryItemAsync_ShouldReturnError_WhenConcurrencyConflict), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });

        var pantryItem = new PantryItem
        {
            Id = Constants.PantryItem.Id,
            HouseholdId = Constants.Household.Id,
            Name = Constants.PantryItem.Name,
            Quantity = Constants.PantryItem.Quantity,
            Unit = Constants.PantryItem.Unit,
            CreatedBy = Constants.User.Id,
            CreatedAt = DateTime.UtcNow,
            RowVersion = Constants.RowVersion.Default
        };

        db.PantryItems.Add(pantryItem);
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        var request = new UpdatePantryItemRequestDto(
            Constants.UpdatedPantryItem.Name,
            Constants.UpdatedPantryItem.Quantity,
            Constants.UpdatedPantryItem.Unit,
            null,
            null,
            null,
            null,
            Constants.RowVersion.Mismatched);

        var result = await service.UpdatePantryItemAsync(Constants.PantryItem.Id, request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(PantryErrors.PantryItemConcurrencyConflict);
    }

    [Fact]
    public async Task DeletePantryItemAsync_ShouldDeleteItem_WhenItemExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(DeletePantryItemAsync_ShouldDeleteItem_WhenItemExists), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });

        var pantryItem = new PantryItem
        {
            Id = Constants.PantryItem.Id,
            HouseholdId = Constants.Household.Id,
            Name = Constants.PantryItem.Name,
            Quantity = Constants.PantryItem.Quantity,
            Unit = Constants.PantryItem.Unit,
            CreatedBy = Constants.User.Id,
            CreatedAt = DateTime.UtcNow,
            RowVersion = Constants.RowVersion.Default
        };

        db.PantryItems.Add(pantryItem);
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        var request = new DeletePantryItemRequestDto(Constants.PantryItem.Id);
        var result = await service.DeletePantryItemAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(Constants.PantryItem.Id);

        var deletedItem = await db.PantryItems.FirstOrDefaultAsync(p => p.Id == Constants.PantryItem.Id);
        deletedItem.ShouldBeNull();
    }

    [Fact]
    public async Task DeletePantryItemAsync_ShouldReturnError_WhenItemNotFound()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(DeletePantryItemAsync_ShouldReturnError_WhenItemNotFound), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        var nonExistentItemId = Constants.NonExistentItemId;
        var request = new DeletePantryItemRequestDto(nonExistentItemId);
        var result = await service.DeletePantryItemAsync(request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(PantryErrors.PantryItemNotFound);
    }

    [Fact]
    public async Task ListPantryItemsAsync_ShouldReturnItems_WhenItemsExist()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(ListPantryItemsAsync_ShouldReturnItems_WhenItemsExist), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });

        db.PantryItems.AddRange(
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.Household.Id,
                Name = Constants.ListItems.Item1Name,
                Quantity = Constants.ListItems.Item1Quantity,
                Unit = Constants.ListItems.Item1Unit,
                CreatedBy = Constants.User.Id,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.RowVersion.Version1
            },
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.Household.Id,
                Name = Constants.ListItems.Item2Name,
                Quantity = Constants.ListItems.Item2Quantity,
                Unit = Constants.ListItems.Item2Unit,
                CreatedBy = Constants.User.Id,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.RowVersion.Version2
            });

        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        var request = new ListPantryItemsRequestDto(null, null, Constants.Pagination.Page1, Constants.Pagination.PageSize50);
        var result = await service.ListPantryItemsAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task ListPantryItemsAsync_ShouldFilterByCategory_WhenCategoryProvided()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(ListPantryItemsAsync_ShouldFilterByCategory_WhenCategoryProvided), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });

        db.PantryItems.AddRange(
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.Household.Id,
                Name = Constants.FilterItems.AppleName,
                Quantity = Constants.ListItems.Item1Quantity,
                Unit = Constants.ListItems.Item1Unit,
                Category = Constants.FilterItems.FruitsCategory,
                CreatedBy = Constants.User.Id,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.RowVersion.Version1
            },
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.Household.Id,
                Name = Constants.FilterItems.BreadName,
                Quantity = Constants.ListItems.Item1Quantity,
                Unit = Constants.ListItems.Item1Unit,
                Category = Constants.FilterItems.BakeryCategory,
                CreatedBy = Constants.User.Id,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.RowVersion.Version2
            });

        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        var request = new ListPantryItemsRequestDto(Constants.FilterItems.FruitsCategory, null, Constants.Pagination.Page1, Constants.Pagination.PageSize50);
        var result = await service.ListPantryItemsAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items.First().Name.ShouldBe(Constants.FilterItems.AppleName);
    }

    [Fact]
    public async Task ListPantryItemsAsync_ShouldFilterBySearchTerm_WhenSearchTermProvided()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(ListPantryItemsAsync_ShouldFilterBySearchTerm_WhenSearchTermProvided), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });

        db.PantryItems.AddRange(
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.Household.Id,
                Name = Constants.FilterItems.AppleName,
                Quantity = Constants.ListItems.Item1Quantity,
                Unit = Constants.ListItems.Item1Unit,
                CreatedBy = Constants.User.Id,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.RowVersion.Version1
            },
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.Household.Id,
                Name = Constants.FilterItems.BananaName,
                Quantity = Constants.ListItems.Item1Quantity,
                Unit = Constants.ListItems.Item1Unit,
                CreatedBy = Constants.User.Id,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.RowVersion.Version2
            });

        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        var request = new ListPantryItemsRequestDto(null, Constants.FilterItems.AppleName, Constants.Pagination.Page1, Constants.Pagination.PageSize50);
        var result = await service.ListPantryItemsAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items.First().Name.ShouldBe(Constants.FilterItems.AppleName);
    }

    [Fact]
    public async Task ListPantryItemsAsync_ShouldBeCaseInsensitive_WhenSearchTermProvided()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.User.Id, Constants.User.Email);
        await using var db = TestHelper.CreateInMemoryContext(nameof(ListPantryItemsAsync_ShouldBeCaseInsensitive_WhenSearchTermProvided), userContext);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = DateTime.UtcNow
        });

        db.PantryItems.AddRange(
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.Household.Id,
                Name = Constants.SearchCaseInsensitive.MilkName,
                Quantity = Constants.ListItems.Item1Quantity,
                Unit = Constants.ListItems.Item1Unit,
                CreatedBy = Constants.User.Id,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.RowVersion.Version1
            },
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.Household.Id,
                Name = Constants.FilterItems.BreadName,
                Quantity = Constants.ListItems.Item1Quantity,
                Unit = Constants.ListItems.Item1Unit,
                CreatedBy = Constants.User.Id,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.RowVersion.Version2
            });

        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, _logger);

        // Search with lowercase should find Milk
        var request = new ListPantryItemsRequestDto(null, Constants.SearchCaseInsensitive.SearchLower, Constants.Pagination.Page1, Constants.Pagination.PageSize50);
        var result = await service.ListPantryItemsAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items.First().Name.ShouldBe(Constants.SearchCaseInsensitive.MilkName);

        // Search with uppercase should also find Milk
        var requestUpper = new ListPantryItemsRequestDto(null, Constants.SearchCaseInsensitive.SearchUpper, Constants.Pagination.Page1, Constants.Pagination.PageSize50);
        var resultUpper = await service.ListPantryItemsAsync(requestUpper, CancellationToken.None);

        resultUpper.IsError.ShouldBeFalse();
        resultUpper.Value.Items.Count.ShouldBe(1);
        resultUpper.Value.Items.First().Name.ShouldBe(Constants.SearchCaseInsensitive.MilkName);
    }
}

