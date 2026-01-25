using Microsoft.Extensions.DependencyInjection;
using PantryCloud.SharedKernel.Exceptions;
using PantryCloud.ShoppingList.Presentation.Mapping;

namespace PantryCloud.ShoppingList.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddAutoMapper(cfg => cfg.AddProfile<ShoppingListMappingProfile>());
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }
}

