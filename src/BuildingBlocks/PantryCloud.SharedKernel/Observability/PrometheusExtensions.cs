using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace PantryCloud.SharedKernel.Observability;

public static class PrometheusExtensions
{
    public static IServiceCollection AddPrometheusMetrics(this IServiceCollection services)
    {
        return services;
    }

    public static WebApplication MapPrometheusMetrics(this WebApplication app)
    {
        app.UseHttpMetrics();
        app.MapMetrics();
        return app;
    }
}
