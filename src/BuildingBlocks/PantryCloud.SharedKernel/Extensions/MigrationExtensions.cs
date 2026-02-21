using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PantryCloud.SharedKernel.Extensions;

public static class MigrationExtensions
{
    private const int MaxAttempts = 6;
    private const int StartupTimeoutSeconds = 120;
    private const int BackoffBaseSeconds = 1;
    private const int BackoffMaxSeconds = 30;

    /// <summary>
    /// Applies pending EF Core migrations at startup with bounded retry and timeout.
    /// Uses exponential backoff when the database is temporarily unavailable (e.g. not ready in Kubernetes).
    /// Startup is cancelled after 120s or when the host is stopping.
    /// </summary>
    public static async Task ApplyMigrationsAsync<TContext>(this IHost host)
        where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("PantryCloud.Migrations");
        var lifetime = scope.ServiceProvider.GetService<IHostApplicationLifetime>();

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(StartupTimeoutSeconds));
        using var linkedCts = lifetime?.ApplicationStopping != null
            ? CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, lifetime.ApplicationStopping)
            : null;
        var ct = linkedCts?.Token ?? timeoutCts.Token;

        Exception? lastException = null;
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync(ct);
                logger?.LogInformation("EF Core migrations applied for {Context}", typeof(TContext).Name);
                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt == MaxAttempts)
                {
                    break;
                }
                
                var delay = Math.Min(BackoffBaseSeconds * (1 << (attempt - 1)), BackoffMaxSeconds);
                logger?.LogWarning(ex, "Migration attempt {Attempt}/{Max} failed, retrying in {Delay}s", attempt, MaxAttempts, delay);
                await Task.Delay(TimeSpan.FromSeconds(delay), ct);
            }
        }

        throw lastException!;
    }
}
