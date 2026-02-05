using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Audit.Core.Entities;
using PantryCloud.Audit.Infrastructure.Persistence;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Audit.Infrastructure.Consumers;

public class PantryItemDeletedAuditConsumer(
    AuditDbContext dbContext,
    ILogger<PantryItemDeletedAuditConsumer> logger)
    : DbContextConsumerBase<PantryItemDeletedEvent, AuditDbContext>(dbContext, logger)
{
    protected override async Task HandleAsync(PantryItemDeletedEvent @event, ConsumeContext context)
    {
        if (await dbContext.HouseholdAuditEntries.AnyAsync(e => e.EventId == @event.Id, context.CancellationToken))
        {
            Logger.LogDebug("Event {EventId} already processed, skipping", @event.Id);
            return;
        }

        var payload = JsonSerializer.Serialize(new { @event.ItemName });
        var entry = new HouseholdAuditEntry
        {
            Id = Guid.NewGuid(),
            HouseholdId = @event.HouseholdId,
            ActionType = "Deleted",
            EntityType = "PantryItem",
            EntityId = @event.ItemId,
            UserId = @event.InitiatedByUserId,
            Payload = payload,
            OccurredAt = @event.OccurredAt,
            CorrelationId = @event.CorrelationId,
            EventId = @event.Id
        };

        await dbContext.HouseholdAuditEntries.AddAsync(entry, context.CancellationToken);
        await dbContext.SaveChangesAsync(context.CancellationToken);

        Logger.LogDebug("Recorded audit for PantryItemDeletedEvent - ItemId: {ItemId}", @event.ItemId);
    }
}
