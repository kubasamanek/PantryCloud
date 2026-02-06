using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Audit.Core.Entities;
using PantryCloud.Audit.Infrastructure.Persistence;
using PantryCloud.ShoppingList.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Audit.Infrastructure.Consumers;

public class ShoppingListCreatedAuditConsumer(
    AuditDbContext dbContext,
    ILogger<ShoppingListCreatedAuditConsumer> logger)
    : DbContextConsumerBase<ShoppingListCreatedEvent, AuditDbContext>(dbContext, logger)
{
    private readonly AuditDbContext _dbContext = dbContext;

    protected override async Task HandleAsync(ShoppingListCreatedEvent @event, ConsumeContext context)
    {
        if (await _dbContext.HouseholdAuditEntries.AnyAsync(e => e.EventId == @event.Id, context.CancellationToken))
        {
            Logger.LogDebug("Event {EventId} already processed, skipping", @event.Id);
            return;
        }

        var payload = JsonSerializer.Serialize(new { @event.ShoppingListName, @event.CreatedByUserId });
        var entry = new HouseholdAuditEntry
        {
            Id = Guid.NewGuid(),
            HouseholdId = @event.HouseholdId,
            ActionType = "Created",
            EntityType = "ShoppingList",
            EntityId = @event.ShoppingListId,
            UserId = @event.CreatedByUserId,
            Payload = payload,
            OccurredAt = @event.OccurredAt,
            CorrelationId = @event.CorrelationId,
            EventId = @event.Id
        };

        await _dbContext.HouseholdAuditEntries.AddAsync(entry, context.CancellationToken);
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        Logger.LogDebug("Recorded audit for ShoppingListCreatedEvent - ShoppingListId: {ShoppingListId}", @event.ShoppingListId);
    }
}
