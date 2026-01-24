using Microsoft.Extensions.DependencyInjection;

namespace PantryCloud.SharedKernel.Correlation;

/// <summary>
/// Extension methods for registering correlation ID services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds correlation ID services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
        return services;
    }
}


