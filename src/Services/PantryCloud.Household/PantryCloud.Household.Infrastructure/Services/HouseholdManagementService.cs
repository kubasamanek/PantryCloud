using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Core.Entities;
using PantryCloud.Household.Core.Enums;
using PantryCloud.Household.Core.Errors;
using PantryCloud.Household.Infrastructure.Persistence;

namespace PantryCloud.Household.Infrastructure.Services;

public class HouseholdManagementService(HouseholdDbContext dbContext, IUserContext userContext, ILogger<HouseholdManagementService> logger) : IHouseholdManagementService
{
    public async Task<ErrorOr<GetCurrentHouseholdResponseDto>> GetCurrentHousehold(CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        logger.LogInformation("Getting current household for {UserId}", userId);
        
        var household = await dbContext.Households
            .Where(h => h.Members.Any(m => m.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);

        if (household is null)
        {
            logger.LogInformation("Household for {UserId} not found", userId);
            return Error.NotFound("Household.NotFound", "Household not found");
        }
        
        logger.LogInformation("Got current household for {UserId}", userId);

        return new GetCurrentHouseholdResponseDto(household.Id, household.Name);
    }

    public async Task<ErrorOr<CreateHouseholdResponseDto>> CreateHousehold(CreateHouseholdRequestDto request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        logger.LogInformation("Creating household for {UserId}", userId);
        
        var alreadyMember = await dbContext.Members
            .AnyAsync(m => m.UserId == userId, cancellationToken);

        if (alreadyMember)
        {
            logger.LogInformation("Household for {UserId} already exists", userId);
            return HouseholdErrors.UserAlreadyInHousehold;
        }

        // Create new entities
        var household = new Core.Entities.Household
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
        };

        var member = new HouseholdMember
        {
            Id = Guid.NewGuid(),
            HouseholdId = household.Id,
            UserId = userId,
            Role = HouseholdRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        dbContext.Households.Add(household);
        dbContext.Members.Add(member);

        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Created household for {UserId}", userId);

        return new CreateHouseholdResponseDto(household.Id, household.Name);
    }
    
    public async Task<ErrorOr<LeaveHouseholdResponseDto>> LeaveHousehold(LeaveHouseholdRequestDto request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        logger.LogInformation("User {UserId} requested to leave household", userId);

        var member = await dbContext.Members
            .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);

        if (member is null)
        {
            logger.LogWarning("User {UserId} is not a member of any household", userId);
            return HouseholdErrors.UserNotInAnyHousehold;
        }

        var householdId = member.HouseholdId;

        var membersInHousehold = await dbContext.Members
            .Where(m => m.HouseholdId == householdId)
            .ToListAsync(cancellationToken);

        // User is the only member → delete household and member
        if (membersInHousehold.Count == 1)
        {
            dbContext.Members.Remove(member);

            var household = await dbContext.Households.FirstOrDefaultAsync(h => h.Id == householdId, cancellationToken);
            if (household is not null)
            {
                dbContext.Households.Remove(household);
            }

            logger.LogInformation("User {UserId} was the only member. Household deleted.", userId);
        }
        // User is owner but not the only member → can't leave
        else if (member.Role == HouseholdRole.Owner)
        {
            logger.LogWarning("Owner {UserId} cannot leave household with other members present", userId);
            return HouseholdErrors.OwnerCannotLeave;
        }
        // Normal case: user is member/admin and can leave
        else
        {
            dbContext.Members.Remove(member);
            logger.LogInformation("User {UserId} left the household", userId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new LeaveHouseholdResponseDto(householdId, userContext.Email, DateTime.Now);
    }
}