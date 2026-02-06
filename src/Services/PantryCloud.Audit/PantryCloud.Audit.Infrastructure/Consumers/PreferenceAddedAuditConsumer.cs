using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Audit.Core.Entities;
using PantryCloud.Audit.Infrastructure.Persistence;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Audit.Infrastructure.Consumers;

public class PreferenceAddedAuditConsumer(
    AuditDbContext dbContext,
    ILogger<PreferenceAddedAuditConsumer> logger)
    : DbContextConsumerBase<PreferenceAddedEvent, AuditDbContext>(dbContext, logger)
{
    private readonly AuditDbContext _dbContext = dbContext;

    protected override async Task HandleAsync(PreferenceAddedEvent @event, ConsumeContext context)
    {
        if (await _dbContext.HouseholdAuditEntries.AnyAsync(e => e.EventId == @event.Id, context.CancellationToken))
        {
            Logger.LogDebug("Event {EventId} already processed, skipping", @event.Id);
            return;
        }

        var payload = JsonSerializer.Serialize(new { @event.UserId });
        var entry = new HouseholdAuditEntry
        {
            Id = Guid.NewGuid(),
            HouseholdId = @event.HouseholdId,
            ActionType = "PreferenceAdded",
            EntityType = "Preference",
            EntityId = @event.UserId,
            UserId = @event.UserId,
            Payload = payload,
            OccurredAt = @event.OccurredAt,
            CorrelationId = @event.CorrelationId,
            EventId = @event.Id
        };

        await _dbContext.HouseholdAuditEntries.AddAsync(entry, context.CancellationToken);
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        Logger.LogDebug("Recorded audit for PreferenceAddedEvent - UserId: {UserId}", @event.UserId);
    }
}
