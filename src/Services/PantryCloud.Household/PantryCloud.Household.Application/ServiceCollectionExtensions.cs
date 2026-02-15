using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Household.Application.Queries;

namespace PantryCloud.Household.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetCurrentHouseholdQuery).Assembly));

        //services.AddValidatorsFromAssembly(typeof(RegisterCommand).Assembly);
        //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}