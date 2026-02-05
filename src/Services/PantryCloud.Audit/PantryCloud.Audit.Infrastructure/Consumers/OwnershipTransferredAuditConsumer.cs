using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Audit.Core.Entities;
using PantryCloud.Audit.Infrastructure.Persistence;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Audit.Infrastructure.Consumers;

public class OwnershipTransferredAuditConsumer(
    AuditDbContext dbContext,
    ILogger<OwnershipTransferredAuditConsumer> logger)
    : DbContextConsumerBase<OwnershipTransferredEvent, AuditDbContext>(dbContext, logger)
{
    protected override async Task HandleAsync(OwnershipTransferredEvent @event, ConsumeContext context)
    {
        if (await dbContext.HouseholdAuditEntries.AnyAsync(e => e.EventId == @event.Id, context.CancellationToken))
        {
            Logger.LogDebug("Event {EventId} already processed, skipping", @event.Id);
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            @event.PreviousOwnerId,
            @event.NewOwnerId,
            @event.NewOwnerEmail
        });
        var entry = new HouseholdAuditEntry
        {
            Id = Guid.NewGuid(),
            HouseholdId = @event.HouseholdId,
            ActionType = "OwnershipTransferred",
            EntityType = "Household",
            EntityId = @event.HouseholdId,
            UserId = @event.NewOwnerId,
            Payload = payload,
            OccurredAt = @event.TransferredAt,
            CorrelationId = @event.CorrelationId,
            EventId = @event.Id
        };

        await dbContext.HouseholdAuditEntries.AddAsync(entry, context.CancellationToken);
        await dbContext.SaveChangesAsync(context.CancellationToken);

        Logger.LogDebug("Recorded audit for OwnershipTransferredEvent - HouseholdId: {HouseholdId}", @event.HouseholdId);
    }
}
