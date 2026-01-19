namespace PantryCloud.Pantry.Application;

public interface IHouseholdCacheHydrationService
{
    Task<bool> HydrateCacheForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> HydrateAllCacheAsync(CancellationToken cancellationToken);
}

