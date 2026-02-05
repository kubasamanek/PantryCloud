using Microsoft.Extensions.DependencyInjection;

namespace PantryCloud.Audit.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IAuditQueryService).Assembly));
        return services;
    }
}
