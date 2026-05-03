using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.SharedKernel.Persistence;

/// <summary>
/// Extension methods for registering persistence-related services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="AuditableEntityInterceptor"/> to the service collection and configures it for the specified DbContext.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureDbContext">The action to configure the DbContext options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDbContextWithAuditing<TDbContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
        where TDbContext : DbContext
    {
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<TDbContext>((serviceProvider, options) =>
        {
            configureDbContext(options);
            var interceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();
            options.AddInterceptors(interceptor);
        });

        return services;
    }

    /// <summary>
    /// Registers the <see cref="OutboxWriter{TDbContext}"/> and <see cref="OutboxRelayWorker{TDbContext}"/>
    /// so that integration events written via <see cref="IOutboxWriter"/> are relayed to the message broker
    /// by a background worker.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext that owns the OutboxMessages table.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional override for relay worker options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOutboxRelay<TDbContext>(
        this IServiceCollection services,
        Action<OutboxRelayOptions>? configureOptions = null)
        where TDbContext : DbContext
    {
        var options = new OutboxRelayOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);
        services.AddScoped<IOutboxWriter, OutboxWriter<TDbContext>>();
        services.AddHostedService<OutboxRelayWorker<TDbContext>>();

        return services;
    }
}
