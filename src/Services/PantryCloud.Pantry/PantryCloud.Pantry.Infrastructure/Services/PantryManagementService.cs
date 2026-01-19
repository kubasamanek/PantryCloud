using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Pantry.Application;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Core.Errors;
using PantryCloud.Pantry.Infrastructure.Persistence;

namespace PantryCloud.Pantry.Infrastructure.Services;

public class PantryManagementService(
    PantryDbContext dbContext,
    IUserContext userContext,
    IHouseholdCacheHydrationService cacheHydrationService,
    ILogger<PantryManagementService> logger) : IPantryManagementService
{
    public async Task<ErrorOr<CreatePantryItemResponseDto>> CreatePantryItemAsync(CreatePantryItemRequestDto request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        logger.LogInformation("Creating pantry item {Name} for user {UserId}", request.Name, userId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(userId, cancellationToken);
        if (householdIdResult.IsError)
        {
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var pantryItem = new PantryItem
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            Name = request.Name,
            Quantity = request.Quantity,
            Unit = request.Unit,
            ExpirationDate = request.ExpirationDate,
            Category = request.Category,
            Notes = request.Notes,
            ImageUrl = request.ImageUrl,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.PantryItems.AddAsync(pantryItem, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created pantry item {ItemId} for user {UserId}", pantryItem.Id, userId);

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
        var userId = userContext.UserId;

        logger.LogInformation("Updating pantry item {ItemId} for user {UserId}", id, userId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(userId, cancellationToken);
        if (householdIdResult.IsError)
        {
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var pantryItem = await dbContext.PantryItems
            .FirstOrDefaultAsync(p => p.Id == id && p.HouseholdId == householdId, cancellationToken);

        if (pantryItem == null)
        {
            logger.LogWarning("Pantry item {ItemId} not found for user {UserId}", id, userId);
            return PantryErrors.PantryItemNotFound;
        }

        if (!pantryItem.RowVersion.SequenceEqual(request.RowVersion))
        {
            logger.LogWarning("Concurrency conflict detected for pantry item {ItemId}", id);
            return PantryErrors.PantryItemConcurrencyConflict;
        }

        pantryItem.Name = request.Name;
        pantryItem.Quantity = request.Quantity;
        pantryItem.Unit = request.Unit;
        pantryItem.ExpirationDate = request.ExpirationDate;
        pantryItem.Category = request.Category;
        pantryItem.Notes = request.Notes;
        pantryItem.ImageUrl = request.ImageUrl;
        pantryItem.ModifiedBy = userId;
        pantryItem.ModifiedAt = DateTime.UtcNow;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning("Concurrency conflict detected for pantry item {ItemId} during save", id);
            return PantryErrors.PantryItemConcurrencyConflict;
        }

        logger.LogInformation("Updated pantry item {ItemId} for user {UserId}", id, userId);

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
        var userId = userContext.UserId;

        logger.LogInformation("Deleting pantry item {ItemId} for user {UserId}", request.Id, userId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(userId, cancellationToken);
        if (householdIdResult.IsError)
        {
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var pantryItem = await dbContext.PantryItems
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.HouseholdId == householdId, cancellationToken);

        if (pantryItem == null)
        {
            logger.LogWarning("Pantry item {ItemId} not found for user {UserId}", request.Id, userId);
            return PantryErrors.PantryItemNotFound;
        }

        dbContext.PantryItems.Remove(pantryItem);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted pantry item {ItemId} for user {UserId}", request.Id, userId);

        return new DeletePantryItemResponseDto(request.Id);
    }

    public async Task<ErrorOr<GetPantryItemResponseDto>> GetPantryItemAsync(GetPantryItemRequestDto request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        logger.LogInformation("Getting pantry item {ItemId} for user {UserId}", request.Id, userId);

        var householdIdResult = await GetCurrentHouseholdIdAsync(userId, cancellationToken);
        if (householdIdResult.IsError)
        {
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var pantryItem = await dbContext.PantryItems
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.HouseholdId == householdId, cancellationToken);

        if (pantryItem == null)
        {
            logger.LogWarning("Pantry item {ItemId} not found for user {UserId}", request.Id, userId);
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
        var userId = userContext.UserId;

        logger.LogInformation("Listing pantry items for user {UserId} with filters: Category={Category}, SearchTerm={SearchTerm}", userId, request.Category, request.SearchTerm);

        var householdIdResult = await GetCurrentHouseholdIdAsync(userId, cancellationToken);
        if (householdIdResult.IsError)
        {
            return householdIdResult.Errors;
        }
        var householdId = householdIdResult.Value;

        var query = dbContext.PantryItems
            .Where(p => p.HouseholdId == householdId);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(p => p.Category == request.Category);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(request.SearchTerm));
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

        logger.LogInformation("Found {Count} pantry items for user {UserId}", totalCount, userId);

        return new ListPantryItemsResponseDto(items, totalCount, request.Page, request.PageSize);
    }

    private async Task<ErrorOr<Guid>> GetCurrentHouseholdIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var membership = await dbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(
                m => m.UserId == userId && m.LeftAt == null,
                cancellationToken);

        if (membership != null)
        {
            logger.LogDebug("Household ID resolved from cache for user {UserId}: {HouseholdId}", userId, membership.HouseholdId);
            return membership.HouseholdId;
        }

        logger.LogInformation("Cache miss for user {UserId}. Attempting to hydrate cache from Household service.", userId);
        
        var hydrated = await cacheHydrationService.HydrateCacheForUserAsync(userId, cancellationToken);
        if (hydrated)
        {
            membership = await dbContext.UserHouseholdMemberships
                .FirstOrDefaultAsync(
                    m => m.UserId == userId && m.LeftAt == null,
                    cancellationToken);
            
            if (membership != null)
            {
                logger.LogInformation("Cache hydrated successfully for user {UserId}: {HouseholdId}", userId, membership.HouseholdId);
                return membership.HouseholdId;
            }
        }

        logger.LogWarning("User {UserId} does not belong to any household", userId);
        return PantryErrors.HouseholdNotFound;
    }
}

