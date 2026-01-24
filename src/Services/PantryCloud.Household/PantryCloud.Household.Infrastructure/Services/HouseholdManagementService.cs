using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Core.Entities;
using PantryCloud.Household.Core.Enums;
using PantryCloud.Household.Core.Errors;
using PantryCloud.Household.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Services;

namespace PantryCloud.Household.Infrastructure.Services;

public class HouseholdManagementService(
    HouseholdDbContext dbContext,
    IUserContext userContext,
    ILogger<HouseholdManagementService> logger) 
    : BaseDbContextService<HouseholdManagementService, HouseholdDbContext>(dbContext, userContext, logger), IHouseholdManagementService
{
    public async Task<ErrorOr<GetCurrentHouseholdResponseDto>> GetCurrentHousehold(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Getting current household for {UserId}", UserId);
        
        var household = await DbContext.Households
            .Where(h => h.Members.Any(m => m.UserId == UserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (household is null)
        {
            Logger.LogInformation("Household for {UserId} not found", UserId);
            return Error.NotFound("Household.NotFound", "Household not found");
        }
        
        Logger.LogInformation("Got current household for {UserId}", UserId);

        return new GetCurrentHouseholdResponseDto(household.Id, household.Name);
    }

    public async Task<ErrorOr<GetHouseholdByUserIdResponseDto>> GetHouseholdByUserId(GetHouseholdByUserIdRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Getting household for user {UserId}", request.UserId);
        
        var household = await DbContext.Households
            .Where(h => h.Members.Any(m => m.UserId == request.UserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (household is null)
        {
            Logger.LogInformation("Household for user {UserId} not found", request.UserId);
            return Error.NotFound("Household.NotFound", "Household not found");
        }
        
        Logger.LogInformation("Got household for user {UserId}", request.UserId);

        return new GetHouseholdByUserIdResponseDto(household.Id, household.Name);
    }

    public async Task<ErrorOr<CreateHouseholdResponseDto>> CreateHousehold(CreateHouseholdRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Creating household for {UserId}", UserId);
        
        var alreadyMember = await DbContext.Members
            .AnyAsync(m => m.UserId == UserId, cancellationToken);

        if (alreadyMember)
        {
            Logger.LogInformation("Household for {UserId} already exists", UserId);
            return HouseholdErrors.UserAlreadyInHousehold;
        }

        var household = new Core.Entities.Household
        {
            Name = request.Name,
        };

        var member = new HouseholdMember
        {
            HouseholdId = household.Id,
            UserId = UserId,
            Role = HouseholdRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        DbContext.Households.Add(household);
        DbContext.Members.Add(member);

        await DbContext.SaveChangesAsync(cancellationToken);
        
        Logger.LogInformation("Created household for {UserId}", UserId);

        return new CreateHouseholdResponseDto(
            household.Id, 
            household.Name, 
            member.UserId, 
            UserEmail, 
            member.JoinedAt);
    }
    
    public async Task<ErrorOr<LeaveHouseholdResponseDto>> LeaveHousehold(LeaveHouseholdRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("User {UserId} requested to leave household", UserId);

        var member = await DbContext.Members
            .FirstOrDefaultAsync(m => m.UserId == UserId, cancellationToken);

        if (member is null)
        {
            Logger.LogWarning("User {UserId} is not a member of any household", UserId);
            return HouseholdErrors.UserNotInAnyHousehold;
        }

        var householdId = member.HouseholdId;

        var membersInHousehold = await DbContext.Members
            .Where(m => m.HouseholdId == householdId)
            .ToListAsync(cancellationToken);

        // User is the only member → delete household and member
        if (membersInHousehold.Count == 1)
        {
            DbContext.Members.Remove(member);

            var household = await DbContext.Households.FirstOrDefaultAsync(h => h.Id == householdId, cancellationToken);
            if (household is not null)
            {
                DbContext.Households.Remove(household);
            }

            Logger.LogInformation("User {UserId} was the only member. Household deleted.", UserId);
        }
        // User is owner but not the only member → can't leave
        else if (member.Role == HouseholdRole.Owner)
        {
            Logger.LogWarning("Owner {UserId} cannot leave household with other members present", UserId);
            return HouseholdErrors.OwnerCannotLeave;
        }
        // Normal case: user is member/admin and can leave
        else
        {
            DbContext.Members.Remove(member);
            Logger.LogInformation("User {UserId} left the household", UserId);
        }

        await DbContext.SaveChangesAsync(cancellationToken);

        return new LeaveHouseholdResponseDto(householdId, UserId, UserEmail, DateTime.UtcNow);
    }
}