using Microsoft.EntityFrameworkCore;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Infrastructure.Persistence;

namespace PantryCloud.Notification.Infrastructure.Persistence;

public class HouseholdMembershipRepository(NotificationDbContext dbContext) : IHouseholdMembershipRepository
{
    public async Task<IReadOnlyList<Guid>> GetHouseholdMemberIdsAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserHouseholdMemberships
            .Where(m => m.HouseholdId == householdId && m.LeftAt == null)
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetHouseholdMemberIdsExceptAsync(Guid householdId, Guid excludeUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserHouseholdMemberships
            .Where(m => m.HouseholdId == householdId && m.LeftAt == null && m.UserId != excludeUserId)
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);
    }
}
