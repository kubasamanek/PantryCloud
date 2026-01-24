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
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(CreatePantryItemAsync_ShouldCreateItem_WhenUserHasHousehold), userContext);
        var cacheService = TestHelper.MockCacheHydrationService();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        var request = new CreatePantryItemRequestDto(
            Constants.PantryItemName,
            Constants.PantryItemQuantity,
            Constants.PantryItemUnit,
            Constants.PantryItemExpirationDate,
            Constants.PantryItemCategory,
            Constants.PantryItemNotes,
            Constants.PantryItemImageUrl);

        var result = await service.CreatePantryItemAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Name.ShouldBe(Constants.PantryItemName);
        result.Value.Quantity.ShouldBe(Constants.PantryItemQuantity);
        result.Value.Unit.ShouldBe(Constants.PantryItemUnit);
        result.Value.HouseholdId.ShouldBe(Constants.HouseholdId);
        result.Value.CreatedBy.ShouldBe(Constants.UserId);

        var item = await db.PantryItems.FirstOrDefaultAsync(p => p.Id == result.Value.Id);
        item.ShouldNotBeNull();
        item.Name.ShouldBe(Constants.PantryItemName);
    }

    [Fact]
    public async Task CreatePantryItemAsync_ShouldReturnError_WhenUserNotInHousehold()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(CreatePantryItemAsync_ShouldReturnError_WhenUserNotInHousehold), userContext);
        var cacheService = TestHelper.MockCacheHydrationService(shouldHydrate: false);

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        var request = new CreatePantryItemRequestDto(
            Constants.PantryItemName,
            Constants.PantryItemQuantity,
            Constants.PantryItemUnit,
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
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetPantryItemAsync_ShouldReturnItem_WhenItemExists), userContext);
        var cacheService = TestHelper.MockCacheHydrationService();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            JoinedAt = DateTime.UtcNow
        });

        var pantryItem = new PantryItem
        {
            Id = Constants.PantryItemId,
            HouseholdId = Constants.HouseholdId,
            Name = Constants.PantryItemName,
            Quantity = Constants.PantryItemQuantity,
            Unit = Constants.PantryItemUnit,
            ExpirationDate = Constants.PantryItemExpirationDate,
            Category = Constants.PantryItemCategory,
            Notes = Constants.PantryItemNotes,
            ImageUrl = Constants.PantryItemImageUrl,
            CreatedBy = Constants.UserId,
            CreatedAt = DateTime.UtcNow,
            RowVersion = Constants.DefaultRowVersion
        };

        db.PantryItems.Add(pantryItem);
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        var request = new GetPantryItemRequestDto(Constants.PantryItemId);
        var result = await service.GetPantryItemAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(Constants.PantryItemId);
        result.Value.Name.ShouldBe(Constants.PantryItemName);
        result.Value.Quantity.ShouldBe(Constants.PantryItemQuantity);
    }

    [Fact]
    public async Task GetPantryItemAsync_ShouldReturnError_WhenItemNotFound()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetPantryItemAsync_ShouldReturnError_WhenItemNotFound), userContext);
        var cacheService = TestHelper.MockCacheHydrationService();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        var nonExistentItemId = Guid.NewGuid();
        var request = new GetPantryItemRequestDto(nonExistentItemId);
        var result = await service.GetPantryItemAsync(request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(PantryErrors.PantryItemNotFound);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_ShouldUpdateItem_WhenItemExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdatePantryItemAsync_ShouldUpdateItem_WhenItemExists), userContext);
        var cacheService = TestHelper.MockCacheHydrationService();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            JoinedAt = DateTime.UtcNow
        });

        var pantryItem = new PantryItem
        {
            Id = Constants.PantryItemId,
            HouseholdId = Constants.HouseholdId,
            Name = Constants.PantryItemName,
            Quantity = Constants.PantryItemQuantity,
            Unit = Constants.PantryItemUnit,
            CreatedBy = Constants.UserId,
            CreatedAt = DateTime.UtcNow,
            RowVersion = Constants.DefaultRowVersion
        };

        db.PantryItems.Add(pantryItem);
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        var request = new UpdatePantryItemRequestDto(
            Constants.UpdatedItemName,
            Constants.UpdatedItemQuantity,
            Constants.UpdatedItemUnit,
            Constants.UpdatedItemExpirationDate,
            Constants.UpdatedItemCategory,
            Constants.UpdatedItemNotes,
            Constants.UpdatedItemImageUrl,
            Constants.DefaultRowVersion);

        var result = await service.UpdatePantryItemAsync(Constants.PantryItemId, request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Name.ShouldBe(Constants.UpdatedItemName);
        result.Value.Quantity.ShouldBe(Constants.UpdatedItemQuantity);
        result.Value.Unit.ShouldBe(Constants.UpdatedItemUnit);
        result.Value.ModifiedBy.ShouldBe(Constants.UserId);
        result.Value.ModifiedAt.ShouldNotBeNull();

        var updatedItem = await db.PantryItems.FirstOrDefaultAsync(p => p.Id == Constants.PantryItemId);
        updatedItem.ShouldNotBeNull();
        updatedItem.Name.ShouldBe(Constants.UpdatedItemName);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_ShouldReturnError_WhenItemNotFound()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(UpdatePantryItemAsync_ShouldReturnError_WhenItemNotFound), userContext);
        var cacheService = TestHelper.MockCacheHydrationService();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        var nonExistentItemId = Guid.NewGuid();
        var request = new UpdatePantryItemRequestDto(
            Constants.UpdatedItemName,
            Constants.UpdatedItemQuantity,
            Constants.UpdatedItemUnit,
            null,
            null,
            null,
            null,
            Constants.DefaultRowVersion);

        var result = await service.UpdatePantryItemAsync(nonExistentItemId, request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(PantryErrors.PantryItemNotFound);
    }

    [Fact]
    public async Task DeletePantryItemAsync_ShouldDeleteItem_WhenItemExists()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(DeletePantryItemAsync_ShouldDeleteItem_WhenItemExists), userContext);
        var cacheService = TestHelper.MockCacheHydrationService();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            JoinedAt = DateTime.UtcNow
        });

        var pantryItem = new PantryItem
        {
            Id = Constants.PantryItemId,
            HouseholdId = Constants.HouseholdId,
            Name = Constants.PantryItemName,
            Quantity = Constants.PantryItemQuantity,
            Unit = Constants.PantryItemUnit,
            CreatedBy = Constants.UserId,
            CreatedAt = DateTime.UtcNow,
            RowVersion = Constants.DefaultRowVersion
        };

        db.PantryItems.Add(pantryItem);
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        var request = new DeletePantryItemRequestDto(Constants.PantryItemId);
        var result = await service.DeletePantryItemAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(Constants.PantryItemId);

        var deletedItem = await db.PantryItems.FirstOrDefaultAsync(p => p.Id == Constants.PantryItemId);
        deletedItem.ShouldBeNull();
    }

    [Fact]
    public async Task DeletePantryItemAsync_ShouldReturnError_WhenItemNotFound()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(DeletePantryItemAsync_ShouldReturnError_WhenItemNotFound), userContext);
        var cacheService = TestHelper.MockCacheHydrationService();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        var nonExistentItemId = Guid.NewGuid();
        var request = new DeletePantryItemRequestDto(nonExistentItemId);
        var result = await service.DeletePantryItemAsync(request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(PantryErrors.PantryItemNotFound);
    }

    [Fact]
    public async Task ListPantryItemsAsync_ShouldReturnItems_WhenItemsExist()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(ListPantryItemsAsync_ShouldReturnItems_WhenItemsExist), userContext);
        var cacheService = TestHelper.MockCacheHydrationService();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            JoinedAt = DateTime.UtcNow
        });

        db.PantryItems.AddRange(
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.HouseholdId,
                Name = Constants.TestItem1Name,
                Quantity = Constants.TestItem1Quantity,
                Unit = Constants.TestItem1Unit,
                CreatedBy = Constants.UserId,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.TestRowVersion1
            },
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.HouseholdId,
                Name = Constants.TestItem2Name,
                Quantity = Constants.TestItem2Quantity,
                Unit = Constants.TestItem2Unit,
                CreatedBy = Constants.UserId,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.TestRowVersion2
            });

        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        var request = new ListPantryItemsRequestDto(null, null, 1, 50);
        var result = await service.ListPantryItemsAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task ListPantryItemsAsync_ShouldFilterByCategory_WhenCategoryProvided()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(ListPantryItemsAsync_ShouldFilterByCategory_WhenCategoryProvided), userContext);
        var cacheService = TestHelper.MockCacheHydrationService();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            JoinedAt = DateTime.UtcNow
        });

        db.PantryItems.AddRange(
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.HouseholdId,
                Name = Constants.TestAppleName,
                Quantity = Constants.TestItem1Quantity,
                Unit = Constants.TestItem1Unit,
                Category = Constants.TestFruitsCategory,
                CreatedBy = Constants.UserId,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.TestRowVersion1
            },
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.HouseholdId,
                Name = Constants.TestBreadName,
                Quantity = Constants.TestItem1Quantity,
                Unit = Constants.TestItem1Unit,
                Category = Constants.TestBakeryCategory,
                CreatedBy = Constants.UserId,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.TestRowVersion2
            });

        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        var request = new ListPantryItemsRequestDto(Constants.TestFruitsCategory, null, 1, 50);
        var result = await service.ListPantryItemsAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items.First().Name.ShouldBe(Constants.TestAppleName);
    }

    [Fact]
    public async Task ListPantryItemsAsync_ShouldFilterBySearchTerm_WhenSearchTermProvided()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(ListPantryItemsAsync_ShouldFilterBySearchTerm_WhenSearchTermProvided), userContext);
        var cacheService = TestHelper.MockCacheHydrationService();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            JoinedAt = DateTime.UtcNow
        });

        db.PantryItems.AddRange(
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.HouseholdId,
                Name = Constants.TestAppleName,
                Quantity = Constants.TestItem1Quantity,
                Unit = Constants.TestItem1Unit,
                CreatedBy = Constants.UserId,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.TestRowVersion1
            },
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.HouseholdId,
                Name = Constants.TestBananaName,
                Quantity = Constants.TestItem1Quantity,
                Unit = Constants.TestItem1Unit,
                CreatedBy = Constants.UserId,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.TestRowVersion2
            });

        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        var request = new ListPantryItemsRequestDto(null, Constants.TestAppleName, 1, 50);
        var result = await service.ListPantryItemsAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items.First().Name.ShouldBe(Constants.TestAppleName);
    }

    [Fact]
    public async Task ListPantryItemsAsync_ShouldBeCaseInsensitive_WhenSearchTermProvided()
    {
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);
        await using var db = TestHelper.CreateInMemoryContext(nameof(ListPantryItemsAsync_ShouldBeCaseInsensitive_WhenSearchTermProvided), userContext);
        var cacheService = TestHelper.MockCacheHydrationService();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            JoinedAt = DateTime.UtcNow
        });

        db.PantryItems.AddRange(
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.HouseholdId,
                Name = "Milk",
                Quantity = Constants.TestItem1Quantity,
                Unit = Constants.TestItem1Unit,
                CreatedBy = Constants.UserId,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.TestRowVersion1
            },
            new PantryItem
            {
                Id = Guid.NewGuid(),
                HouseholdId = Constants.HouseholdId,
                Name = "Bread",
                Quantity = Constants.TestItem1Quantity,
                Unit = Constants.TestItem1Unit,
                CreatedBy = Constants.UserId,
                CreatedAt = DateTime.UtcNow,
                RowVersion = Constants.TestRowVersion2
            });

        await db.SaveChangesAsync();

        var service = new PantryManagementService(db, userContext, cacheService, _logger);

        // Search with lowercase should find "Milk"
        var request = new ListPantryItemsRequestDto(null, "milk", 1, 50);
        var result = await service.ListPantryItemsAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items.First().Name.ShouldBe("Milk");

        // Search with uppercase should also find "Milk"
        var requestUpper = new ListPantryItemsRequestDto(null, "MILK", 1, 50);
        var resultUpper = await service.ListPantryItemsAsync(requestUpper, CancellationToken.None);

        resultUpper.IsError.ShouldBeFalse();
        resultUpper.Value.Items.Count.ShouldBe(1);
        resultUpper.Value.Items.First().Name.ShouldBe("Milk");
    }
}

