using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Audit.Core.Entities;
using PantryCloud.Audit.Infrastructure.Persistence;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Audit.Infrastructure.Consumers;

public class MemberJoinedHouseholdAuditConsumer(
    AuditDbContext dbContext,
    ILogger<MemberJoinedHouseholdAuditConsumer> logger)
    : DbContextConsumerBase<MemberJoinedHouseholdEvent, AuditDbContext>(dbContext, logger)
{
    private readonly AuditDbContext _dbContext = dbContext;

    protected override async Task HandleAsync(MemberJoinedHouseholdEvent @event, ConsumeContext context)
    {
        if (await _dbContext.HouseholdAuditEntries.AnyAsync(e => e.EventId == @event.Id, context.CancellationToken))
        {
            Logger.LogDebug("Event {EventId} already processed, skipping", @event.Id);
            return;
        }

        var existingMembership = await _dbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == @event.NewMemberId, context.CancellationToken);

        if (existingMembership != null)
        {
            if (existingMembership.HouseholdId != @event.HouseholdId)
            {
                existingMembership.HouseholdId = @event.HouseholdId;
                existingMembership.JoinedAt = @event.JoinedAt;
                existingMembership.LeftAt = null;
            }
            else if (existingMembership.LeftAt != null)
            {
                existingMembership.LeftAt = null;
                existingMembership.JoinedAt = @event.JoinedAt;
            }
        }
        else
        {
            await _dbContext.UserHouseholdMemberships.AddAsync(new UserHouseholdMembership
            {
                UserId = @event.NewMemberId,
                HouseholdId = @event.HouseholdId,
                JoinedAt = @event.JoinedAt
            }, context.CancellationToken);
        }

        var payload = JsonSerializer.Serialize(new { @event.NewMemberId, @event.MemberEmail });
        var entry = new HouseholdAuditEntry
        {
            Id = Guid.NewGuid(),
            HouseholdId = @event.HouseholdId,
            ActionType = "Joined",
            EntityType = "Member",
            EntityId = @event.NewMemberId,
            UserId = @event.NewMemberId,
            Payload = payload,
            OccurredAt = @event.JoinedAt,
            CorrelationId = @event.CorrelationId,
            EventId = @event.Id
        };

        await _dbContext.HouseholdAuditEntries.AddAsync(entry, context.CancellationToken);
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        Logger.LogDebug("Recorded audit for MemberJoinedHouseholdEvent - NewMemberId: {NewMemberId}", @event.NewMemberId);
    }
}
