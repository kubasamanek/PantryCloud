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
        Logger.LogInformation(
            "Received MemberJoinedHouseholdEvent - UserId: {UserId}, HouseholdId: {HouseholdId}, JoinedAt: {JoinedAt}, MessageId: {MessageId}",
            @event.NewMemberId,
            @event.HouseholdId,
            @event.JoinedAt,
            context.MessageId);

        var existingMembership = await DbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == @event.NewMemberId, context.CancellationToken);

        if (existingMembership != null)
        {
            Logger.LogInformation(
                "Found existing membership for UserId: {UserId}, Current HouseholdId: {CurrentHouseholdId}, LeftAt: {LeftAt}",
                @event.NewMemberId,
                existingMembership.HouseholdId,
                existingMembership.LeftAt);

            if (existingMembership.HouseholdId != @event.HouseholdId)
            {
                Logger.LogInformation(
                    "User switched households - Moving from HouseholdId: {OldHouseholdId} to HouseholdId: {NewHouseholdId}",
                    existingMembership.HouseholdId,
                    @event.HouseholdId);

                existingMembership.HouseholdId = @event.HouseholdId;
                existingMembership.JoinedAt = @event.JoinedAt;
                existingMembership.LeftAt = null;
            }
            else
            {
                // User is rejoining the same household
                if (existingMembership.LeftAt != null)
                {
                    Logger.LogInformation(
                        "User rejoining same household - HouseholdId: {HouseholdId}, Previously left at: {LeftAt}",
                        @event.HouseholdId,
                        existingMembership.LeftAt);

                    existingMembership.LeftAt = null;
                    existingMembership.JoinedAt = @event.JoinedAt;
                }
                else
                {
                    Logger.LogInformation(
                        "User already active in household - HouseholdId: {HouseholdId}, no changes needed",
                        @event.HouseholdId);
                }
            }
        }
        else
        {
            Logger.LogInformation(
                "Creating new membership - UserId: {UserId}, HouseholdId: {HouseholdId}",
                @event.NewMemberId,
                @event.HouseholdId);

            // New membership
            var membership = new UserHouseholdMembership
            {
                UserId = @event.NewMemberId,
                HouseholdId = @event.HouseholdId,
                JoinedAt = @event.JoinedAt
            };

            await DbContext.UserHouseholdMemberships.AddAsync(membership, context.CancellationToken);
        }

        Logger.LogInformation(
            "Successfully processed MemberJoinedHouseholdEvent for UserId: {UserId}, HouseholdId: {HouseholdId}",
            @event.NewMemberId,
            @event.HouseholdId);
    }
}
