using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PantryCloud.Pantry.Application;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.Pantry.Core.Options;
using PantryCloud.Pantry.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Infrastructure.Services;

public class ExpirationCheckService(
    PantryDbContext dbContext,
    IOutboxWriter outboxWriter,
    IOptions<ExpirationCheckOptions> options,
    TimeProvider timeProvider,
    ILogger<ExpirationCheckService> logger) : IExpirationCheckService
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var opts = options.Value;
        var now = timeProvider.GetUtcNow();
        var today = DateOnly.FromDateTime(now.DateTime);
        var tomorrow = today.AddDays(1);
        var dayAfterTomorrow = today.AddDays(2);

        var todayStart = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayAfterTomorrowEnd = dayAfterTomorrow.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        logger.LogInformation("Checking expiration check for today {today}", today);

        var items = await dbContext.PantryItems
            .AsNoTracking()
            .Where(p => p.ExpirationDate != null
                && p.ExpirationDate >= todayStart
                && p.ExpirationDate <= dayAfterTomorrowEnd)
            .Select(p => new { p.HouseholdId, p.Id, p.Name, p.ExpirationDate })
            .ToListAsync(cancellationToken);

        var grouped = items
            .GroupBy(x => x.HouseholdId)
            .ToList();

        if (grouped.Count == 0)
        {
            logger.LogDebug("No expiring pantry items found");
            return;
        }

        logger.LogInformation(
            "Found {HouseholdCount} households with {ItemCount} expiring items",
            grouped.Count,
            items.Count);

        var batchSize = Math.Max(1, opts.BatchSize);
        var batchNumber = 0;

        foreach (var batch in grouped.Chunk(batchSize))
        {
            batchNumber++;
            if (batchNumber > 1 && opts.BatchDelayMs > 0)
            {
                await Task.Delay(opts.BatchDelayMs, cancellationToken);
            }

            foreach (var householdGroup in batch)
            {
                var expiringItems = householdGroup
                    .Select(x => new ExpiringItemDto(
                        x.Id,
                        x.Name,
                        x.ExpirationDate!.Value,
                        GetDaysUntilExpiry(x.ExpirationDate.Value, today, tomorrow, dayAfterTomorrow)))
                    .ToList();

                var @event = new PantryItemsExpiringSoonEvent
                {
                    HouseholdId = householdGroup.Key,
                    Items = expiringItems,
                    CorrelationId = Guid.NewGuid().ToString()
                };

                await outboxWriter.WriteAsync(@event, cancellationToken);
                logger.LogDebug(
                    "Queued PantryItemsExpiringSoonEvent to outbox for household {HouseholdId} with {ItemCount} items",
                    householdGroup.Key,
                    expiringItems.Count);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static int GetDaysUntilExpiry(DateTime expirationDate, DateOnly today, DateOnly tomorrow, DateOnly dayAfterTomorrow)
    {
        var expDate = DateOnly.FromDateTime(expirationDate);
        if (expDate == today) return 0;
        if (expDate == tomorrow) return 1;
        if (expDate == dayAfterTomorrow) return 2;
        return 0;
    }
}
