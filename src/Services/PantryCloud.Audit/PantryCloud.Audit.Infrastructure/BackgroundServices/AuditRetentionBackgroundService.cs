using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PantryCloud.Audit.Core.Options;
using PantryCloud.Audit.Infrastructure.Persistence;

namespace PantryCloud.Audit.Infrastructure.BackgroundServices;

public class AuditRetentionBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<AuditOptions> options,
    ILogger<AuditRetentionBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retentionDays = Math.Max(1, options.Value.RetentionDays);
        var interval = TimeSpan.FromHours(24);

        logger.LogInformation("Audit retention job started - deleting entries older than {RetentionDays} days", retentionDays);

        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

                var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
                var deleted = await dbContext.HouseholdAuditEntries
                    .Where(e => e.OccurredAt < cutoff)
                    .ExecuteDeleteAsync(stoppingToken);

                if (deleted > 0)
                    logger.LogInformation("Audit retention: deleted {Count} entries older than {Cutoff}", deleted, cutoff);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during audit retention cleanup");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));

        logger.LogInformation("Audit retention job stopped");
    }
}
