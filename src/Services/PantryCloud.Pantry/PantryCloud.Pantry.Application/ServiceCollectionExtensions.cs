using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Pantry.Application.Commands;

namespace PantryCloud.Pantry.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePantryItemCommand).Assembly));
        
        return services;
    }
}

