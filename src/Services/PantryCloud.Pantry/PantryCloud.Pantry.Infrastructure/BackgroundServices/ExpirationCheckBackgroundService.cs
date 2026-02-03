using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PantryCloud.Pantry.Application;
using PantryCloud.Pantry.Core.Options;

namespace PantryCloud.Pantry.Infrastructure.BackgroundServices;

public class ExpirationCheckBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<ExpirationCheckOptions> options,
    ILogger<ExpirationCheckBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = options.Value;
        if (!opts.Enabled)
        {
            logger.LogInformation("Expiration check is disabled");
            return;
        }

        var interval = opts.DebugEnabled
            ? TimeSpan.FromSeconds(30)
            : TimeSpan.FromHours(Math.Max(1, opts.IntervalHours));
        logger.LogInformation(
            opts.DebugEnabled
                ? "Expiration check started in DEBUG mode with 30-second interval"
                : "Expiration check started with interval of {IntervalHours} hours",
            opts.IntervalHours);

        using var timer = new PeriodicTimer(interval);

        while (true)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<IExpirationCheckService>();
                await service.RunAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running expiration check");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }

        logger.LogInformation("Expiration check stopped");
    }
}
