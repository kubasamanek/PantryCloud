using System.Security.Cryptography;
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

public class InvitationService(
    ILogger<InvitationService> logger,
    IUserContext userContext,
    HouseholdDbContext dbContext)
    : BaseDbContextService<InvitationService, HouseholdDbContext>(dbContext, userContext, logger), IInvitationService
{
    public async Task<ErrorOr<SendHouseholdInvitationResponseDto>> SendHouseholdInvitation(SendHouseholdInvitationRequestDto request, CancellationToken cancellationToken)
    {
        var user = DbContext.Members.FirstOrDefault(u => u.UserId == UserId);

        var invitationHouseholdId = new Guid(request.HouseholdId);

        if (user == null)
        {
            Logger.LogWarning("User {Email} is not in any household and cannot send household invitation.", UserEmail);
            return InvitationErrors.UserNotInHousehold;
        }

        if (user.HouseholdId != invitationHouseholdId)
        {
            Logger.LogWarning("User {Email} is not a member of this household and cannot send household invitation.", UserEmail);
            return InvitationErrors.UserNotHouseholdMember;
        }

        if (user.Role != HouseholdRole.Owner)
        {
            Logger.LogWarning("User {Email} is not owner of the household and cannot send household invitation.", UserEmail);
            return InvitationErrors.UserNotHouseholdOwner;
        }

        var existingInvitation = await DbContext.Invitations
            .FirstOrDefaultAsync(x =>
                x.Email == request.ToEmail &&
                x.HouseholdId == invitationHouseholdId &&
                x.ExpiresAt > DateTime.UtcNow &&
                x.UsedAt == null, cancellationToken: cancellationToken);

        if (existingInvitation != null)
        {
            Logger.LogInformation("There is already a pending invitation from  {Email} to {ExistingInvitationEmail}", UserEmail, existingInvitation.Email);
            return new SendHouseholdInvitationResponseDto(existingInvitation.Code);
        }

        var newInvitation = new HouseholdInvitation
        {
            Code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Email = request.ToEmail,
            HouseholdId = invitationHouseholdId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(2)
        };

        await DbContext.Invitations.AddAsync(newInvitation, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("User {Email} sent new invitation to {NewInvitationEmail}", UserEmail, newInvitation.Email);

        return new SendHouseholdInvitationResponseDto(newInvitation.Code);
    }

    public async Task<ErrorOr<AcceptHouseholdInvitationResponseDto>> AcceptHouseholdInvitation(AcceptHouseholdInvitationRequestDto request, CancellationToken cancellationToken)
    {
        var code = request.Code;

        var invitation = DbContext.Invitations.FirstOrDefault(x => x.Code == code);

        if (invitation == null)
        {
            Logger.LogWarning("Invitation {Code} not found.", code);
            return InvitationErrors.InvalidInvitation;
        }

        if (invitation.Email != UserEmail)
        {
            Logger.LogWarning("Invitation {Code} is not for this user.", code);
            return InvitationErrors.InvitationNotForUser(UserEmail);
        }

        if (invitation.IsExpired)
        {
            Logger.LogWarning("Invitation {Code} is expired.", code);
            return InvitationErrors.ExpiredInvitation;
        }

        if (invitation.IsUsed)
        {
            Logger.LogWarning("Invitation {Code} is used already.", code);
            return InvitationErrors.UsedInvitation;
        }

        var user = DbContext.Members.FirstOrDefault(u => u.UserId == UserId);

        if (user != null && user.Role == HouseholdRole.Owner)
        {
            Logger.LogWarning("Owner {Email} cannot accept invitation - must transfer ownership first.", UserEmail);
            return InvitationErrors.OwnerCannotAcceptInvitation;
        }

        invitation.UsedAt = DateTime.UtcNow;

        if (user != null)
        {
            Logger.LogInformation("User left household and joined another one.");
            user.HouseholdId = invitation.HouseholdId;
            await DbContext.SaveChangesAsync(cancellationToken);
            // LEAVE CURRENT HOUSEHOLD
            // TODO: Can't when owner
            // TODO: other constraints
            return new AcceptHouseholdInvitationResponseDto(
                HouseholdId: invitation.HouseholdId,
                MemberId: UserId,
                MemberEmail: invitation.Email,
                JoinedAt: DateTime.UtcNow);
        }

        var newMember = new HouseholdMember
        {
            HouseholdId = invitation.HouseholdId,
            JoinedAt = DateTime.UtcNow,
            Role = HouseholdRole.Member,
            UserId = UserId,
        };

        await DbContext.Members.AddAsync(newMember, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("User joined a new household.");
        return new AcceptHouseholdInvitationResponseDto(
            HouseholdId: invitation.HouseholdId,
            MemberId: UserId,
            MemberEmail: invitation.Email,
            JoinedAt: newMember.JoinedAt);
    }
}