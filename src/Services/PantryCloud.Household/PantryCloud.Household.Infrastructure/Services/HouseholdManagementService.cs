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

    public async Task<ErrorOr<KickMemberResponseDto>> KickMemberAsync(KickMemberRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("User {UserId} requested to kick member {MemberUserId}", UserId, request.MemberUserId);

        var owner = await DbContext.Members
            .FirstOrDefaultAsync(m => m.UserId == UserId, cancellationToken);

        if (owner is null)
        {
            Logger.LogWarning("User {UserId} is not a member of any household", UserId);
            return HouseholdErrors.UserNotInAnyHousehold;
        }

        if (owner.Role != HouseholdRole.Owner)
        {
            Logger.LogWarning("User {UserId} is not owner and cannot kick members", UserId);
            return HouseholdErrors.UserNotOwner;
        }

        if (request.MemberUserId == UserId)
        {
            Logger.LogWarning("Owner {UserId} cannot kick themselves", UserId);
            return HouseholdErrors.CannotKickSelf;
        }

        var targetMember = await DbContext.Members
            .FirstOrDefaultAsync(m => m.UserId == request.MemberUserId && m.HouseholdId == owner.HouseholdId, cancellationToken);

        if (targetMember is null)
        {
            Logger.LogWarning("User {MemberUserId} is not a member of household {HouseholdId}", request.MemberUserId, owner.HouseholdId);
            return HouseholdErrors.MemberNotFoundInHousehold;
        }

        if (targetMember.Role == HouseholdRole.Owner)
        {
            Logger.LogWarning("Cannot kick the household owner {MemberUserId}", request.MemberUserId);
            return HouseholdErrors.CannotKickOwner;
        }

        var kickedAt = DateTime.UtcNow;
        DbContext.Members.Remove(targetMember);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("User {UserId} kicked member {MemberUserId} from household {HouseholdId}", UserId, request.MemberUserId, owner.HouseholdId);

        return new KickMemberResponseDto(owner.HouseholdId, request.MemberUserId, kickedAt);
    }

    public async Task<ErrorOr<TransferOwnershipResponseDto>> TransferOwnershipAsync(TransferOwnershipRequestDto request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("User {UserId} requested to transfer ownership to {NewOwnerUserId}", UserId, request.NewOwnerUserId);

        var currentOwner = await DbContext.Members
            .FirstOrDefaultAsync(m => m.UserId == UserId, cancellationToken);

        if (currentOwner is null)
        {
            Logger.LogWarning("User {UserId} is not a member of any household", UserId);
            return HouseholdErrors.UserNotInAnyHousehold;
        }

        if (currentOwner.Role != HouseholdRole.Owner)
        {
            Logger.LogWarning("User {UserId} is not owner and cannot transfer ownership", UserId);
            return HouseholdErrors.UserNotOwner;
        }

        if (request.NewOwnerUserId == UserId)
        {
            Logger.LogWarning("Owner {UserId} cannot transfer ownership to themselves", UserId);
            return HouseholdErrors.CannotTransferToSelf;
        }

        var newOwnerMember = await DbContext.Members
            .FirstOrDefaultAsync(m => m.UserId == request.NewOwnerUserId && m.HouseholdId == currentOwner.HouseholdId, cancellationToken);

        if (newOwnerMember is null)
        {
            Logger.LogWarning("User {NewOwnerUserId} is not a member of household {HouseholdId}", request.NewOwnerUserId, currentOwner.HouseholdId);
            return HouseholdErrors.NewOwnerMustBeMember;
        }

        var transferredAt = DateTime.UtcNow;
        currentOwner.Role = HouseholdRole.Member;
        newOwnerMember.Role = HouseholdRole.Owner;
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Ownership transferred from {UserId} to {NewOwnerUserId} in household {HouseholdId}", UserId, request.NewOwnerUserId, currentOwner.HouseholdId);

        return new TransferOwnershipResponseDto(
            currentOwner.HouseholdId,
            UserId,
            request.NewOwnerUserId,
            transferredAt);
    }
}