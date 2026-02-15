using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.SharedKernel.Identity;

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
}
