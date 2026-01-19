using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Pantry.Infrastructure.Persistence;

namespace PantryCloud.Pantry.Infrastructure.Consumers;

public class MemberLeftHouseholdConsumer(
    PantryDbContext dbContext,
    ILogger<MemberLeftHouseholdConsumer> logger) : IConsumer<MemberLeftHouseholdEvent>
{
    public async Task Consume(ConsumeContext<MemberLeftHouseholdEvent> context)
    {
        var @event = context.Message;
        
        logger.LogInformation(
            "Consuming MemberLeftHouseholdEvent - HouseholdId: {HouseholdId}, MemberEmail: {MemberEmail}, CorrelationId: {CorrelationId}",
            @event.HouseholdId,
            @event.MemberEmail,
            @event.CorrelationId);

        var membership = await dbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(
                m => m.HouseholdId == @event.HouseholdId && m.LeftAt == null,
                context.CancellationToken);

        if (membership != null && membership.HouseholdId == @event.HouseholdId)
        {
            membership.LeftAt = @event.LeftAt;
            await dbContext.SaveChangesAsync(context.CancellationToken);
            
            logger.LogInformation(
                "Updated household membership - UserId: {UserId} left HouseholdId: {HouseholdId}",
                membership.UserId,
                @event.HouseholdId);
        }
        else
        {
            logger.LogWarning(
                "No active membership found for HouseholdId: {HouseholdId} in MemberLeftHouseholdEvent",
                @event.HouseholdId);
        }
    }
}
