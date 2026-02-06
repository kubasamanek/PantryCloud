using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Audit.Core.Entities;
using PantryCloud.Audit.Infrastructure.Persistence;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Audit.Infrastructure.Consumers;

public class PantryItemDepletedAuditConsumer(
    AuditDbContext dbContext,
    ILogger<PantryItemDepletedAuditConsumer> logger)
    : DbContextConsumerBase<PantryItemDepletedEvent, AuditDbContext>(dbContext, logger)
{
    private readonly AuditDbContext _dbContext = dbContext;

    protected override async Task HandleAsync(PantryItemDepletedEvent @event, ConsumeContext context)
    {
        if (await _dbContext.HouseholdAuditEntries.AnyAsync(e => e.EventId == @event.Id, context.CancellationToken))
        {
            Logger.LogDebug("Event {EventId} already processed, skipping", @event.Id);
            return;
        }

        var payload = JsonSerializer.Serialize(new { @event.ItemName });
        var entry = new HouseholdAuditEntry
        {
            Id = Guid.NewGuid(),
            HouseholdId = @event.HouseholdId,
            ActionType = "Depleted",
            EntityType = "PantryItem",
            EntityId = @event.ItemId,
            UserId = @event.InitiatedByUserId,
            Payload = payload,
            OccurredAt = @event.OccurredAt,
            CorrelationId = @event.CorrelationId,
            EventId = @event.Id
        };

        await _dbContext.HouseholdAuditEntries.AddAsync(entry, context.CancellationToken);
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        Logger.LogDebug("Recorded audit for PantryItemDepletedEvent - ItemId: {ItemId}", @event.ItemId);
    }
}
