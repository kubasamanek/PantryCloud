namespace PantryCloud.Notification.Application;

/// <summary>
/// Repository for resolving household membership for notification targeting.
/// </summary>
public interface IHouseholdMembershipRepository
{
    /// <summary>
    /// Gets all active member user IDs in the household.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetHouseholdMemberIdsAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active member user IDs in the household, excluding a specific user.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetHouseholdMemberIdsExceptAsync(Guid householdId, Guid excludeUserId, CancellationToken cancellationToken = default);
}
