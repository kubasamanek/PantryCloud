using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Recipe.Application.Commands;

namespace PantryCloud.Recipe.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SeedRecipesCommand).Assembly));
        
        return services;
    }
}

