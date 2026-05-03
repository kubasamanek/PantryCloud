using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Household.Application.Queries;
using PantryCloud.SharedKernel.Behaviors;

namespace PantryCloud.Household.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(GetCurrentHouseholdQuery).Assembly);
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        return services;
    }
}