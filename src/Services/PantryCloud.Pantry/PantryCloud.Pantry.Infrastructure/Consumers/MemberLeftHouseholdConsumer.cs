using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Pantry.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Infrastructure.Consumers;

public class MemberLeftHouseholdConsumer(
    PantryDbContext dbContext,
    ILogger<MemberLeftHouseholdConsumer> logger) 
    : DbContextConsumerBase<MemberLeftHouseholdEvent, PantryDbContext>(dbContext, logger)
{
    protected override async Task HandleAsync(MemberLeftHouseholdEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received MemberLeftHouseholdEvent - UserId: {UserId}, HouseholdId: {HouseholdId}, LeftAt: {LeftAt}, MessageId: {MessageId}",
            @event.MemberId,
            @event.HouseholdId,
            @event.LeftAt,
            context.MessageId);

        var membership = await DbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(
                m => m.UserId == @event.MemberId && m.HouseholdId == @event.HouseholdId && m.LeftAt == null,
                context.CancellationToken);

        if (membership != null)
        {
            Logger.LogInformation(
                "Found active membership - MembershipId: {MembershipId}, JoinedAt: {JoinedAt}",
                membership.Id,
                membership.JoinedAt);

            membership.LeftAt = @event.LeftAt;
            
            Logger.LogInformation(
                "Updated household membership - UserId: {UserId} left HouseholdId: {HouseholdId} at {LeftAt}",
                @event.MemberId,
                @event.HouseholdId,
                @event.LeftAt);
        }
        else
        {
            Logger.LogWarning(
                "No active membership found for UserId: {UserId}, HouseholdId: {HouseholdId} in MemberLeftHouseholdEvent",
                @event.MemberId,
                @event.HouseholdId);
        }
    }
}
