using PantryCloud.SharedKernel.Exceptions;
using PantryCloud.SharedKernel.Extensions;

namespace PantryCloud.ApiGateway.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationLayerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var enableSwagger = configuration.GetValue("Gateway:EnableSwagger", false);
        
        if (enableSwagger)
        {
            services.AddSwaggerGenWithAuth();
            services.AddEndpointsApiExplorer();
        }

        services.AddControllers();
        
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}

