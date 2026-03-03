using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Pantry.Application.Commands;
using PantryCloud.SharedKernel.Behaviors;

namespace PantryCloud.Pantry.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreatePantryItemCommand).Assembly);
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        return services;
    }
}
