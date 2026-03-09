using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MassTransit;
using PantryCloud.SharedKernel.Outbox;

namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Background worker that polls the <see cref="OutboxMessage"/> table and publishes pending events to the broker.
/// </summary>
public class OutboxRelayWorker<TDbContext>(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxRelayWorker<TDbContext>> logger,
    OutboxRelayOptions options) : BackgroundService
    where TDbContext : DbContext
{
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "OutboxRelayWorker<{DbContext}> started. PollInterval: {PollIntervalSeconds}s, BatchSize: {BatchSize}",
            typeof(TDbContext).Name,
            options.PollInterval.TotalSeconds,
            options.BatchSize);

        // Main loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(options.PollInterval, stoppingToken);
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown requested, exit loop.
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "OutboxRelayWorker<{DbContext}>: unhandled exception in polling loop",
                    typeof(TDbContext).Name);
            }
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // FOR UPDATE SKIP LOCKED guarantees that each row is exclusively claimed by one replica,
        // preventing duplicate publishes in a horizontally-scaled deployment.
        var pending = await dbContext.Set<OutboxMessage>()
            .FromSqlInterpolated($"""
                SELECT * FROM "OutboxMessages"
                WHERE "ProcessedAt" IS NULL
                ORDER BY "OccurredAt"
                LIMIT {options.BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(stoppingToken);

        if (pending.Count == 0)
        {
            logger.LogDebug(
                "OutboxRelayWorker<{DbContext}>: no pending messages",
                typeof(TDbContext).Name);
            return;
        }

        logger.LogInformation(
            "OutboxRelayWorker<{DbContext}>: processing {Count} pending outbox message(s)",
            typeof(TDbContext).Name,
            pending.Count);

        foreach (var message in pending)
        {
            await PublishSingleAsync(message, publishEndpoint, dbContext, stoppingToken);
        }
    }

    private async Task PublishSingleAsync(
        OutboxMessage message,
        IPublishEndpoint publishEndpoint,
        TDbContext dbContext,
        CancellationToken stoppingToken)
    {
        try
        {
            var eventType = Type.GetType(message.EventType);
            if (eventType is null)
            {
                logger.LogError(
                    "OutboxRelayWorker<{DbContext}>: unknown EventType '{EventType}' for OutboxMessage {Id} — marking as error",
                    typeof(TDbContext).Name,
                    message.EventType,
                    message.Id);

                message.Error = $"Unknown EventType: {message.EventType}";
                message.ProcessedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(stoppingToken);
                return;
            }

            var payload = JsonSerializer.Deserialize(message.Payload, eventType, _serializerOptions);
            await publishEndpoint.Publish(payload!, eventType, stoppingToken);

            message.ProcessedAt = DateTime.UtcNow;
            message.Error = null;

            logger.LogInformation(
                "OutboxRelayWorker<{DbContext}>: published OutboxMessage {Id} ({EventType})",
                typeof(TDbContext).Name,
                message.Id,
                message.EventType);
        }
        catch (Exception ex)
        {
            message.Error = ex.Message;
            logger.LogError(
                ex,
                "OutboxRelayWorker<{DbContext}>: failed to publish OutboxMessage {Id} ({EventType})",
                typeof(TDbContext).Name,
                message.Id,
                message.EventType);
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }
}
