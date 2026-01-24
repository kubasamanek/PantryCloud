using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Infrastructure.Consumers;

public class MemberJoinedHouseholdConsumer(
    PantryDbContext dbContext,
    ILogger<MemberJoinedHouseholdConsumer> logger) 
    : DbContextConsumerBase<MemberJoinedHouseholdEvent, PantryDbContext>(dbContext, logger)
{
    protected override async Task HandleAsync(MemberJoinedHouseholdEvent @event, ConsumeContext context)
    {
        var existingMembership = await DbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == @event.NewMemberId, context.CancellationToken);

        if (existingMembership != null)
        {
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
            
            await DbContext.UserHouseholdMemberships.AddAsync(membership, context.CancellationToken);
        }
    }
}
