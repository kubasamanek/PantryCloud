using System.Security.Cryptography;
using ErrorOr;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Core.Entities;
using PantryCloud.Household.Core.Enums;
using PantryCloud.Household.Core.Errors;
using PantryCloud.Household.Infrastructure.Persistence;

namespace PantryCloud.Household.Infrastructure.Services;

public class InvitationService(ILogger<InvitationService> logger, 
    IUserContext userContext, 
    HouseholdDbContext dbContext) : IInvitationService
{
    public async Task<ErrorOr<SendHouseholdInvitationResponseDto>> SendHouseholdInvitation(SendHouseholdInvitationRequestDto request, CancellationToken cancellationToken)
    {
        var email = userContext.Email;
        var user = dbContext.Members.FirstOrDefault(u => u.UserId == userContext.UserId);

        var invitationHouseholdId = new Guid(request.HouseholdId);
        
        if (user == null)
        {
            logger.LogWarning("User {Email} is not in any household and cannot send household invitation.", email);
            return InvitationErrors.UserNotInHousehold;
        }

        if (user.HouseholdId != invitationHouseholdId)
        {
            logger.LogWarning("User {Email} is not a member of this household and cannot send household invitation.", email);
            return InvitationErrors.UserNotHouseholdMember;
        }
        
        if (user.Role != HouseholdRole.Owner)
        {
            logger.LogWarning("User {Email} is not owner of the household and cannot send household invitation.", email);
            return InvitationErrors.UserNotHouseholdOwner;
        }
        
        var existingInvitation = dbContext.Invitations
            .AsEnumerable() 
            .FirstOrDefault(x =>
                x.Email == request.ToEmail &&
                x.HouseholdId == invitationHouseholdId &&
                x is { IsExpired: false, IsUsed: false });
        
        if (existingInvitation != null)
        {
            logger.LogInformation("There is already a pending invitation from  {Email} to {ExistingInvitationEmail}", email, existingInvitation.Email);
            return new SendHouseholdInvitationResponseDto(existingInvitation.Code);
        }
        
        var newInvitation = new HouseholdInvitation
        {
            Code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Email = request.ToEmail,
            HouseholdId = invitationHouseholdId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(2),
        };
        
        await dbContext.Invitations.AddAsync(newInvitation, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {Email} sent new invitation to {NewInvitationEmail}", email, newInvitation.Email);
        
        return new SendHouseholdInvitationResponseDto(newInvitation.Code);
    }

    public async Task<ErrorOr<AcceptHouseholdInvitationResponseDto>> AcceptHouseholdInvitation(AcceptHouseholdInvitationRequestDto request, CancellationToken cancellationToken)
    {
        var email = userContext.Email;
        var userId = userContext.UserId;
        var code = request.Code;
        
        var invitation = dbContext.Invitations.FirstOrDefault(x => x.Code == code);

        if (invitation == null)
        {
            logger.LogWarning("Invitation {Code} not found.", code);
            return InvitationErrors.InvalidInvitation;
        }

        if (invitation.Email != email)
        {
            logger.LogWarning("Invitation {Code} is not for this user.", code);
            return InvitationErrors.InvitationNotForUser(email);
        }
        
        if (invitation.IsExpired)
        {
            logger.LogWarning("Invitation {Code} is expired.", code);
            return InvitationErrors.ExpiredInvitation;
        }
        
        if (invitation.IsUsed)
        {
            logger.LogWarning("Invitation {Code} is used already.", code);
            return InvitationErrors.UsedInvitation;
        }
        
        var household = dbContext.Households.FirstOrDefault(x => x.Id == invitation.HouseholdId);
        var user = dbContext.Members.FirstOrDefault(u => u.UserId == userContext.UserId);

        invitation.UsedAt = DateTime.UtcNow;
        
        if (user != null)
        {
            logger.LogInformation("User left household and joined another one.");
            user.HouseholdId = invitation.HouseholdId;
            // LEAVE CURRENT HOUSEHOLD
            // TODO: Can't when owner
            // TODO: other constraints
            return new AcceptHouseholdInvitationResponseDto();
        }

        var newMember = new HouseholdMember
        {
            HouseholdId = invitation.HouseholdId,
            JoinedAt = DateTime.UtcNow,
            Role = HouseholdRole.Member,
            UserId = userId,
        };
        
        await dbContext.Members.AddAsync(newMember, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("User joined a new household.");
        return new AcceptHouseholdInvitationResponseDto();
    }
}