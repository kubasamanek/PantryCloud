using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Infrastructure.Persistence;

namespace PantryCloud.Pantry.Infrastructure.Consumers;

public class MemberJoinedHouseholdConsumer(
    PantryDbContext dbContext,
    ILogger<MemberJoinedHouseholdConsumer> logger) : IConsumer<MemberJoinedHouseholdEvent>
{
    public async Task Consume(ConsumeContext<MemberJoinedHouseholdEvent> context)
    {
        var @event = context.Message;
        
        logger.LogInformation(
            "Consuming MemberJoinedHouseholdEvent - HouseholdId: {HouseholdId}, MemberId: {MemberId}, CorrelationId: {CorrelationId}",
            @event.HouseholdId,
            @event.NewMemberId,
            @event.CorrelationId);

        var existingMembership = await dbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == @event.NewMemberId, context.CancellationToken);

        if (existingMembership != null)
        {
            // User switching households - update existing membership
            if (existingMembership.HouseholdId != @event.HouseholdId)
            {
                existingMembership.HouseholdId = @event.HouseholdId;
                existingMembership.JoinedAt = @event.JoinedAt;
                existingMembership.LeftAt = null;
            }
            else
            {
                // User is rejoining the same household
                if (existingMembership.LeftAt != null)
                {
                    existingMembership.LeftAt = null;
                    existingMembership.JoinedAt = @event.JoinedAt;
                }
            }
        }
        else
        {
            // New membership
            var membership = new UserHouseholdMembership
            {
                UserId = @event.NewMemberId,
                HouseholdId = @event.HouseholdId,
                JoinedAt = @event.JoinedAt
            };
            
            await dbContext.UserHouseholdMemberships.AddAsync(membership, context.CancellationToken);
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation(
            "Updated household membership for UserId: {UserId}, HouseholdId: {HouseholdId}",
            @event.NewMemberId,
            @event.HouseholdId);
    }
}
