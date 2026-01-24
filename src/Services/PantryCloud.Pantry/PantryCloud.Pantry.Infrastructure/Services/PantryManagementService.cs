using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Pantry.Application;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Core.Errors;
using PantryCloud.Pantry.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Persistence;
using PantryCloud.SharedKernel.Services;

namespace PantryCloud.Pantry.Infrastructure.Services;

public class PantryManagementService(
    PantryDbContext dbContext,
    IUserContext userContext,
    IHouseholdCacheHydrationService cacheHydrationService,
    ILogger<PantryManagementService> logger) : BaseDbContextService<PantryManagementService, PantryDbContext>(dbContext, userContext, logger), IPantryManagementService
{
    public async Task<ErrorOr<CreatePantryItemResponseDto>> CreatePantryItemAsync(CreatePantryItemRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Creating pantry item {Name} for user {UserId}", request.Name, UserId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(cancellationToken);
        if (householdIdResult.IsError)
        {
            Logger.LogWarning("Failed to get current household ID for user {UserId}", UserId);
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var pantryItem = new PantryItem
        {
            HouseholdId = householdId,
            Name = request.Name,
            Quantity = request.Quantity,
            Unit = request.Unit,
            ExpirationDate = request.ExpirationDate,
            Category = request.Category,
            Notes = request.Notes,
            ImageUrl = request.ImageUrl
        };

        await DbContext.PantryItems.AddAsync(pantryItem, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Created pantry item {ItemId} for user {UserId}", pantryItem.Id, UserId);

        return new CreatePantryItemResponseDto(
            pantryItem.Id,
            pantryItem.HouseholdId,
            pantryItem.Name,
            pantryItem.Quantity,
            pantryItem.Unit,
            pantryItem.ExpirationDate,
            pantryItem.Category,
            pantryItem.Notes,
            pantryItem.ImageUrl,
            pantryItem.CreatedBy,
            pantryItem.CreatedAt);
    }

    public async Task<ErrorOr<UpdatePantryItemResponseDto>> UpdatePantryItemAsync(Guid id, UpdatePantryItemRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Updating pantry item {ItemId} for user {UserId}", id, UserId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(cancellationToken);
        if (householdIdResult.IsError)
        {
            Logger.LogWarning("Failed to get current household ID for user {UserId}", UserId);
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var pantryItem = await DbContext.PantryItems
            .Where(p => p.HouseholdId == householdId && p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (pantryItem == null)
        {
            Logger.LogWarning("Pantry item {ItemId} not found for user {UserId}", id, UserId);
            return PantryErrors.PantryItemNotFound;
        }

        if (!pantryItem.RowVersion.SequenceEqual(request.RowVersion))
        {
            Logger.LogWarning("Concurrency conflict detected for pantry item {ItemId}", id);
            return PantryErrors.PantryItemConcurrencyConflict;
        }

        pantryItem.Name = request.Name;
        pantryItem.Quantity = request.Quantity;
        pantryItem.Unit = request.Unit;
        pantryItem.ExpirationDate = request.ExpirationDate;
        pantryItem.Category = request.Category;
        pantryItem.Notes = request.Notes;
        pantryItem.ImageUrl = request.ImageUrl;

        var result = await ConcurrencyHelper.ExecuteWithConcurrencyHandling(
            async () =>
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                return pantryItem;
            },
            Logger,
            "PantryItem",
            id,
            PantryErrors.PantryItemConcurrencyConflict);

        if (result.IsError)
        {
            return result.Errors;
        }

        Logger.LogInformation("Updated pantry item {ItemId} for user {UserId}", id, UserId);

        return new UpdatePantryItemResponseDto(
            pantryItem.Id,
            pantryItem.HouseholdId,
            pantryItem.Name,
            pantryItem.Quantity,
            pantryItem.Unit,
            pantryItem.ExpirationDate,
            pantryItem.Category,
            pantryItem.Notes,
            pantryItem.ImageUrl,
            pantryItem.CreatedBy,
            pantryItem.CreatedAt,
            pantryItem.ModifiedBy,
            pantryItem.ModifiedAt,
            pantryItem.RowVersion);
    }

    public async Task<ErrorOr<DeletePantryItemResponseDto>> DeletePantryItemAsync(DeletePantryItemRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Deleting pantry item {ItemId} for user {UserId}", request.Id, UserId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(cancellationToken);
        if (householdIdResult.IsError)
        {
            Logger.LogWarning("Failed to get current household ID for user {UserId}", UserId);
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var pantryItem = await DbContext.PantryItems
            .Where(p => p.HouseholdId == householdId && p.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (pantryItem == null)
        {
            Logger.LogWarning("Pantry item {ItemId} not found for user {UserId}", request.Id, UserId);
            return PantryErrors.PantryItemNotFound;
        }

        DbContext.PantryItems.Remove(pantryItem);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Deleted pantry item {ItemId} for user {UserId}", request.Id, UserId);

        return new DeletePantryItemResponseDto(request.Id);
    }

    public async Task<ErrorOr<GetPantryItemResponseDto>> GetPantryItemAsync(GetPantryItemRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Getting pantry item {ItemId} for user {UserId}", request.Id, UserId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(cancellationToken);
        if (householdIdResult.IsError)
        {
            Logger.LogWarning("Failed to get current household ID for user {UserId}", UserId);
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var pantryItem = await DbContext.PantryItems
            .Where(p => p.HouseholdId == householdId && p.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (pantryItem == null)
        {
            Logger.LogWarning("Pantry item {ItemId} not found for user {UserId}", request.Id, UserId);
            return PantryErrors.PantryItemNotFound;
        }

        return new GetPantryItemResponseDto(
            pantryItem.Id,
            pantryItem.HouseholdId,
            pantryItem.Name,
            pantryItem.Quantity,
            pantryItem.Unit,
            pantryItem.ExpirationDate,
            pantryItem.Category,
            pantryItem.Notes,
            pantryItem.ImageUrl,
            pantryItem.CreatedBy,
            pantryItem.CreatedAt,
            pantryItem.ModifiedBy,
            pantryItem.ModifiedAt);
    }

    public async Task<ErrorOr<ListPantryItemsResponseDto>> ListPantryItemsAsync(ListPantryItemsRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Listing pantry items for user {UserId} with filters: Category={Category}, SearchTerm={SearchTerm}", UserId, request.Category, request.SearchTerm);

        var householdIdResult = await GetCurrentHouseholdIdAsync(cancellationToken);
        if (householdIdResult.IsError)
        {
            Logger.LogWarning("Failed to get current household ID for user {UserId}", UserId);
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var query = DbContext.PantryItems
            .Where(p => p.HouseholdId == householdId);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(p => p.Category == request.Category);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.WhereContainsCaseInsensitive(p => p.Name, request.SearchTerm);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PantryItemDto(
                p.Id,
                p.Name,
                p.Quantity,
                p.Unit,
                p.ExpirationDate,
                p.Category,
                p.Notes,
                p.ImageUrl,
                p.CreatedBy,
                p.CreatedAt,
                p.ModifiedBy,
                p.ModifiedAt))
            .ToListAsync(cancellationToken);

        Logger.LogInformation("Found {Count} pantry items for user {UserId}", totalCount, UserId);

        return new ListPantryItemsResponseDto(items, totalCount, request.Page, request.PageSize);
    }

    private async Task<ErrorOr<Guid>> GetCurrentHouseholdIdAsync(CancellationToken cancellationToken)
    {
        var membership = await DbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(
                m => m.UserId == UserId && m.LeftAt == null,
                cancellationToken);

        if (membership != null)
        {
            Logger.LogDebug("Household ID resolved from cache for user {UserId}: {HouseholdId}", UserId, membership.HouseholdId);
            return membership.HouseholdId;
        }

        Logger.LogInformation("Cache miss for user {UserId}. Attempting to hydrate cache from Household service.", UserId);
        
        var hydrated = await cacheHydrationService.HydrateCacheForUserAsync(UserId, cancellationToken);
        if (hydrated)
        {
            membership = await DbContext.UserHouseholdMemberships
                .FirstOrDefaultAsync(
                    m => m.UserId == UserId && m.LeftAt == null,
                    cancellationToken);
            
            if (membership != null)
            {
                Logger.LogInformation("Cache hydrated successfully for user {UserId}: {HouseholdId}", UserId, membership.HouseholdId);
                return membership.HouseholdId;
            }
        }

        Logger.LogWarning("User {UserId} does not belong to any household", UserId);
        return PantryErrors.HouseholdNotFound;
    }
}

