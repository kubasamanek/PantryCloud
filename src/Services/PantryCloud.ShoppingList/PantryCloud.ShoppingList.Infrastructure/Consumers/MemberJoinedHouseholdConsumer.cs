using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.ShoppingList.Core.Entities;
using PantryCloud.ShoppingList.Infrastructure.Persistence;

namespace PantryCloud.ShoppingList.Infrastructure.Consumers;

public class MemberJoinedHouseholdConsumer(
    ShoppingListDbContext dbContext,
    ILogger<MemberJoinedHouseholdConsumer> logger)
    : DbContextConsumerBase<MemberJoinedHouseholdEvent, ShoppingListDbContext>(dbContext, logger)
{
    protected override async Task HandleAsync(MemberJoinedHouseholdEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received MemberJoinedHouseholdEvent for NewMemberId: {NewMemberId}, HouseholdId: {HouseholdId}, JoinedAt: {JoinedAt}",
            @event.NewMemberId, @event.HouseholdId, @event.JoinedAt);

        var existingMembership = await DbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == @event.NewMemberId, context.CancellationToken);

        if (existingMembership != null)
        {
            if (existingMembership.HouseholdId != @event.HouseholdId)
            {
                Logger.LogInformation(
                    "Updating existing membership for UserId: {UserId} to new HouseholdId: {NewHouseholdId} (from old {OldHouseholdId})",
                    @event.NewMemberId, @event.HouseholdId, existingMembership.HouseholdId);
                existingMembership.HouseholdId = @event.HouseholdId;
                existingMembership.JoinedAt = @event.JoinedAt;
                existingMembership.LeftAt = null;
            }
            else
            {
                if (existingMembership.LeftAt != null)
                {
                    Logger.LogInformation(
                        "Reactivating membership for UserId: {UserId} in HouseholdId: {HouseholdId}",
                        @event.NewMemberId, @event.HouseholdId);
                    existingMembership.LeftAt = null;
                    existingMembership.JoinedAt = @event.JoinedAt;
                }
                else
                {
                    Logger.LogDebug(
                        "Membership for UserId: {UserId} in HouseholdId: {HouseholdId} already active and up-to-date. No changes needed.",
                        @event.NewMemberId, @event.HouseholdId);
                }
            }
        }
        else
        {
            Logger.LogInformation(
                "Creating new membership for UserId: {UserId} in HouseholdId: {HouseholdId}",
                @event.NewMemberId, @event.HouseholdId);
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


