using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Persistence;
using PantryCloud.SharedKernel.Services;
using PantryCloud.ShoppingList.Application;
using PantryCloud.ShoppingList.Application.Dtos;
using PantryCloud.ShoppingList.Core.Entities;
using PantryCloud.ShoppingList.Core.Errors;
using PantryCloud.ShoppingList.Infrastructure.Persistence;

namespace PantryCloud.ShoppingList.Infrastructure.Services;

public class ShoppingListManagementService(
    ShoppingListDbContext dbContext,
    IUserContext userContext,
    ILogger<ShoppingListManagementService> logger) 
    : BaseDbContextService<ShoppingListManagementService, ShoppingListDbContext>(dbContext, userContext, logger), IShoppingListManagementService
{
    public async Task<ErrorOr<CreateShoppingListResponseDto>> CreateShoppingListAsync(CreateShoppingListRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Creating shopping list '{Name}' for user {UserId}", request.Name, UserId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(cancellationToken);
        if (householdIdResult.IsError)
        {
            Logger.LogWarning("Failed to get current household ID for user {UserId}", UserId);
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        // Check for duplicate name in household (only active lists)
        var existingList = await DbContext.ShoppingLists
            .AnyAsync(sl => sl.HouseholdId == householdId && sl.Name == request.Name, cancellationToken);

        if (existingList)
        {
            Logger.LogWarning("Shopping list with name '{Name}' already exists in household {HouseholdId}", request.Name, householdId);
            return ShoppingListErrors.DuplicateListName;
        }

        var shoppingList = new Core.Entities.ShoppingList
        {
            HouseholdId = householdId,
            Name = request.Name
        };

        await DbContext.ShoppingLists.AddAsync(shoppingList, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Created shopping list {ListId} for user {UserId}", shoppingList.Id, UserId);

        return new CreateShoppingListResponseDto(
            shoppingList.Id,
            shoppingList.HouseholdId,
            shoppingList.Name,
            shoppingList.CreatedBy,
            shoppingList.CreatedAt);
    }

    public async Task<ErrorOr<DeleteShoppingListResponseDto>> DeleteShoppingListAsync(DeleteShoppingListRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Deleting shopping list {ListId} for user {UserId}", request.Id, UserId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(cancellationToken);
        if (householdIdResult.IsError)
        {
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var shoppingList = await DbContext.ShoppingLists
            .FirstOrDefaultAsync(sl => sl.Id == request.Id && sl.HouseholdId == householdId, cancellationToken);

        if (shoppingList == null)
        {
            Logger.LogWarning("Shopping list {ListId} not found for user {UserId}", request.Id, UserId);
            return ShoppingListErrors.ShoppingListNotFound;
        }

        DbContext.ShoppingLists.Remove(shoppingList);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Deleted shopping list {ListId}", request.Id);

        return new DeleteShoppingListResponseDto(request.Id);
    }

    public async Task<ErrorOr<GetShoppingListResponseDto>> GetShoppingListAsync(GetShoppingListRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Getting shopping list {ListId} for user {UserId}", request.Id, UserId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(cancellationToken);
        if (householdIdResult.IsError)
        {
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var shoppingList = await DbContext.ShoppingLists
            .Include(sl => sl.Items)
            .FirstOrDefaultAsync(sl => sl.Id == request.Id && sl.HouseholdId == householdId, cancellationToken);

        if (shoppingList == null)
        {
            Logger.LogWarning("Shopping list {ListId} not found for user {UserId}", request.Id, UserId);
            return ShoppingListErrors.ShoppingListNotFound;
        }

        var items = shoppingList.Items.Select(i => new ShoppingListItemDto(
            i.Id,
            i.Name,
            i.Quantity,
            i.Unit,
            i.IsChecked,
            i.CheckedBy,
            i.CheckedAt,
            i.Source,
            i.CreatedBy,
            i.CreatedAt,
            i.ModifiedBy,
            i.ModifiedAt,
            i.RowVersion)).ToList();

        return new GetShoppingListResponseDto(
            shoppingList.Id,
            shoppingList.HouseholdId,
            shoppingList.Name,
            shoppingList.CreatedBy,
            shoppingList.CreatedAt,
            shoppingList.ModifiedBy,
            shoppingList.ModifiedAt,
            items);
    }

    public async Task<ErrorOr<ListShoppingListsResponseDto>> ListShoppingListsAsync(ListShoppingListsRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Listing shopping lists for user {UserId}", UserId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(cancellationToken);
        if (householdIdResult.IsError)
        {
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var query = DbContext.ShoppingLists
            .Include(sl => sl.Items)
            .Where(sl => sl.HouseholdId == householdId);

        var totalCount = await query.CountAsync(cancellationToken);

        var lists = await query
            .OrderByDescending(sl => sl.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sl => new ShoppingListSummaryDto(
                sl.Id,
                sl.Name,
                sl.Items.Count,
                sl.Items.Count(i => i.IsChecked),
                sl.CreatedBy,
                sl.CreatedAt,
                sl.ModifiedBy,
                sl.ModifiedAt))
            .ToListAsync(cancellationToken);

        return new ListShoppingListsResponseDto(lists, totalCount, request.Page, request.PageSize);
    }
    
    public async Task<ErrorOr<AddShoppingListItemResponseDto>> AddShoppingListItemAsync(Guid listId, AddShoppingListItemRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Adding item '{Name}' to shopping list {ListId} for user {UserId}", request.Name, listId, UserId);

        var validateListResult = await ValidateListAccessAsync(listId, cancellationToken);
        if (validateListResult.IsError)
        {
            return validateListResult.Errors;
        }

        var item = new ShoppingListItem
        {
            ShoppingListId = listId,
            Name = request.Name,
            Quantity = request.Quantity,
            Unit = request.Unit,
            Source = request.Source
        };

        await DbContext.ShoppingListItems.AddAsync(item, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Added item {ItemId} to shopping list {ListId}", item.Id, listId);

        return new AddShoppingListItemResponseDto(
            item.Id,
            item.ShoppingListId,
            item.Name,
            item.Quantity,
            item.Unit,
            item.IsChecked,
            item.Source,
            item.CreatedBy,
            item.CreatedAt,
            item.RowVersion);
    }

    public async Task<ErrorOr<AddShoppingListItemsBatchResponseDto>> AddShoppingListItemsBatchAsync(Guid listId, AddShoppingListItemsBatchRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Adding {Count} items to shopping list {ListId} for user {UserId}", request.Items.Count, listId, UserId);

        var validateListResult = await ValidateListAccessAsync(listId, cancellationToken);
        if (validateListResult.IsError)
        {
            return validateListResult.Errors;
        }

        var items = request.Items.Select(r => new ShoppingListItem
        {
            ShoppingListId = listId,
            Name = r.Name,
            Quantity = r.Quantity,
            Unit = r.Unit,
            Source = r.Source
        }).ToList();

        await DbContext.ShoppingListItems.AddRangeAsync(items, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Added {Count} items to shopping list {ListId}", items.Count, listId);

        var responseItems = items.Select(i => new AddShoppingListItemResponseDto(
            i.Id,
            i.ShoppingListId,
            i.Name,
            i.Quantity,
            i.Unit,
            i.IsChecked,
            i.Source,
            i.CreatedBy,
            i.CreatedAt,
            i.RowVersion)).ToList();

        return new AddShoppingListItemsBatchResponseDto(responseItems);
    }

    public async Task<ErrorOr<UpdateShoppingListItemResponseDto>> UpdateShoppingListItemAsync(Guid listId, Guid itemId, UpdateShoppingListItemRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Updating item {ItemId} in shopping list {ListId} for user {UserId}", itemId, listId, UserId);

        var validateListResult = await ValidateListAccessAsync(listId, cancellationToken);
        if (validateListResult.IsError)
        {
            return validateListResult.Errors;
        }

        var item = await DbContext.ShoppingListItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ShoppingListId == listId, cancellationToken);

        if (item == null)
        {
            Logger.LogWarning("Item {ItemId} not found in shopping list {ListId}", itemId, listId);
            return ShoppingListErrors.ShoppingListItemNotFound;
        }

        item.Name = request.Name;
        item.Quantity = request.Quantity;
        item.Unit = request.Unit;
        DbContext.Entry(item).Property(i => i.RowVersion).OriginalValue = request.RowVersion;

        try
        {
            await DbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            Logger.LogWarning("Concurrency conflict updating item {ItemId}", itemId);
            return ShoppingListErrors.ShoppingListConcurrencyConflict;
        }

        Logger.LogInformation("Updated item {ItemId}", itemId);

        return new UpdateShoppingListItemResponseDto(
            item.Id,
            item.ShoppingListId,
            item.Name,
            item.Quantity,
            item.Unit,
            item.IsChecked,
            item.CheckedBy,
            item.CheckedAt,
            item.Source,
            item.CreatedBy,
            item.CreatedAt,
            item.ModifiedBy,
            item.ModifiedAt,
            item.RowVersion);
    }

    public async Task<ErrorOr<DeleteShoppingListItemResponseDto>> DeleteShoppingListItemAsync(DeleteShoppingListItemRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Deleting item {ItemId} from shopping list {ListId} for user {UserId}", request.ItemId, request.ListId, UserId);

        var validateListResult = await ValidateListAccessAsync(request.ListId, cancellationToken);
        if (validateListResult.IsError)
        {
            return validateListResult.Errors;
        }

        var item = await DbContext.ShoppingListItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.ShoppingListId == request.ListId, cancellationToken);

        if (item == null)
        {
            Logger.LogWarning("Item {ItemId} not found in shopping list {ListId}", request.ItemId, request.ListId);
            return ShoppingListErrors.ShoppingListItemNotFound;
        }

        DbContext.ShoppingListItems.Remove(item);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Deleted item {ItemId}", request.ItemId);

        return new DeleteShoppingListItemResponseDto(request.ItemId);
    }

    public async Task<ErrorOr<CheckShoppingListItemResponseDto>> CheckShoppingListItemAsync(CheckShoppingListItemRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Toggling check status for item {ItemId} in shopping list {ListId} for user {UserId}", request.ItemId, request.ListId, UserId);

        var validateListResult = await ValidateListAccessAsync(request.ListId, cancellationToken);
        if (validateListResult.IsError)
        {
            return validateListResult.Errors;
        }

        var item = await DbContext.ShoppingListItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.ShoppingListId == request.ListId, cancellationToken);

        if (item == null)
        {
            Logger.LogWarning("Item {ItemId} not found in shopping list {ListId}", request.ItemId, request.ListId);
            return ShoppingListErrors.ShoppingListItemNotFound;
        }

        // Toggle check status
        item.IsChecked = !item.IsChecked;
        item.CheckedBy = item.IsChecked ? UserId : null;
        item.CheckedAt = item.IsChecked ? DateTime.UtcNow : null;

        await DbContext.SaveChangesAsync(cancellationToken);

        var shoppingList = await DbContext.ShoppingLists
            .Include(sl => sl.Items)
            .FirstAsync(sl => sl.Id == request.ListId, cancellationToken);
        var allItemsChecked = shoppingList.Items.Count > 0 && shoppingList.Items.All(i => i.IsChecked);

        Logger.LogInformation("Toggled item {ItemId} check status to {IsChecked}", request.ItemId, item.IsChecked);

        return new CheckShoppingListItemResponseDto(
            item.Id,
            item.IsChecked,
            item.CheckedBy,
            item.CheckedAt,
            AllItemsChecked: allItemsChecked,
            HouseholdId: allItemsChecked ? shoppingList.HouseholdId : null,
            ShoppingListId: allItemsChecked ? shoppingList.Id : null,
            ShoppingListName: allItemsChecked ? shoppingList.Name : null,
            CheckedByUserId: allItemsChecked ? UserId : null);
    }

    // Private Helper Methods

    private async Task<ErrorOr<Guid>> GetCurrentHouseholdIdAsync(CancellationToken cancellationToken)
    {
        var membership = await DbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(
                m => m.UserId == UserId && m.LeftAt == null,
                cancellationToken);

        if (membership == null)
        {
            Logger.LogWarning("User {UserId} does not belong to any household or membership is inactive.", UserId);
            return ShoppingListErrors.HouseholdNotFound;
        }

        Logger.LogDebug("Household ID resolved from local cache for user {UserId}: {HouseholdId}", UserId, membership.HouseholdId);
        return membership.HouseholdId;
    }

    private async Task<ErrorOr<Core.Entities.ShoppingList>> ValidateListAccessAsync(Guid listId, CancellationToken cancellationToken)
    {
        var householdIdResult = await GetCurrentHouseholdIdAsync(cancellationToken);
        if (householdIdResult.IsError)
        {
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var shoppingList = await DbContext.ShoppingLists
            .FirstOrDefaultAsync(sl => sl.Id == listId, cancellationToken);

        if (shoppingList == null)
        {
            Logger.LogWarning("Shopping list {ListId} not found", listId);
            return ShoppingListErrors.ShoppingListNotFound;
        }

        if (shoppingList.HouseholdId != householdId)
        {
            Logger.LogWarning("User {UserId} does not have access to shopping list {ListId}", UserId, listId);
            return ShoppingListErrors.UnauthorizedAccess;
        }

        return shoppingList;
    }
}

