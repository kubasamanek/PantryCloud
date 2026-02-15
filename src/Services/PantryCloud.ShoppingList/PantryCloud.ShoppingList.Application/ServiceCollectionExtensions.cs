using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.ShoppingList.Application.Commands;

namespace PantryCloud.ShoppingList.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateShoppingListCommand).Assembly));

        return services;
    }
}


