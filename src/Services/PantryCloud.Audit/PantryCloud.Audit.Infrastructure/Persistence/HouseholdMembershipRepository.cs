using Microsoft.EntityFrameworkCore;
using PantryCloud.Audit.Core.Entities;

namespace PantryCloud.Audit.Infrastructure.Persistence;

public class HouseholdMembershipRepository(AuditDbContext dbContext) : IHouseholdMembershipRepository
{
    public async Task<bool> IsUserInHouseholdAsync(Guid userId, Guid householdId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserHouseholdMemberships
            .AnyAsync(
                m => m.UserId == userId && m.HouseholdId == householdId && m.LeftAt == null,
                cancellationToken);
    }
}
