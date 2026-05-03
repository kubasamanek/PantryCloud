namespace PantryCloud.Audit.Infrastructure.Persistence;

public interface IHouseholdMembershipRepository
{
    Task<bool> IsUserInHouseholdAsync(Guid userId, Guid householdId, CancellationToken cancellationToken = default);
}
