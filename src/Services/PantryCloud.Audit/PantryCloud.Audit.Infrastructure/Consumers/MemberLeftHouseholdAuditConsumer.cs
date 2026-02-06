using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Audit.Core.Entities;
using PantryCloud.Audit.Infrastructure.Persistence;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Audit.Infrastructure.Consumers;

public class MemberLeftHouseholdAuditConsumer(
    AuditDbContext dbContext,
    ILogger<MemberLeftHouseholdAuditConsumer> logger)
    : DbContextConsumerBase<MemberLeftHouseholdEvent, AuditDbContext>(dbContext, logger)
{
    private readonly AuditDbContext _dbContext = dbContext;

    protected override async Task HandleAsync(MemberLeftHouseholdEvent @event, ConsumeContext context)
    {
        if (await _dbContext.HouseholdAuditEntries.AnyAsync(e => e.EventId == @event.Id, context.CancellationToken))
        {
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            return;
        }
        
        var membership = await _dbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == @event.MemberId && m.LeftAt == null, context.CancellationToken);
        if (membership != null && membership.HouseholdId == @event.HouseholdId)
        {
            membership.LeftAt = @event.LeftAt;
        }

        var payload = JsonSerializer.Serialize(new { @event.MemberId, @event.MemberEmail });
        var entry = new HouseholdAuditEntry
        {
            Id = Guid.NewGuid(),
            HouseholdId = @event.HouseholdId,
            ActionType = "Left",
            EntityType = "Member",
            EntityId = @event.MemberId,
            UserId = @event.MemberId,
            Payload = payload,
            OccurredAt = @event.LeftAt,
            CorrelationId = @event.CorrelationId,
            EventId = @event.Id
        };

        await _dbContext.HouseholdAuditEntries.AddAsync(entry, context.CancellationToken);
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        Logger.LogDebug("Recorded audit for MemberLeftHouseholdEvent - MemberId: {MemberId}", @event.MemberId);
    }
}
