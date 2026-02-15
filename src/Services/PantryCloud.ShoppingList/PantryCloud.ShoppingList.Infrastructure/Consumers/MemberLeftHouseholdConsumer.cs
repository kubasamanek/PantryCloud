using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.ShoppingList.Infrastructure.Persistence;

namespace PantryCloud.ShoppingList.Infrastructure.Consumers;

public class MemberLeftHouseholdConsumer(
    ShoppingListDbContext dbContext,
    ILogger<MemberLeftHouseholdConsumer> logger)
    : DbContextConsumerBase<MemberLeftHouseholdEvent, ShoppingListDbContext>(dbContext, logger)
{
    protected override async Task HandleAsync(MemberLeftHouseholdEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received MemberLeftHouseholdEvent for MemberId: {MemberId}, HouseholdId: {HouseholdId}, LeftAt: {LeftAt}",
            @event.MemberId, @event.HouseholdId, @event.LeftAt);

        var membership = await DbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(
                m => m.UserId == @event.MemberId && m.HouseholdId == @event.HouseholdId && m.LeftAt == null,
                context.CancellationToken);

        if (membership != null)
        {
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
                "No active membership found for UserId: {UserId}, HouseholdId: {HouseholdId} in MemberLeftHouseholdEvent. No update performed.",
                @event.MemberId,
                @event.HouseholdId);
        }
    }
}


