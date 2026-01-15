using PantryCloud.SharedKernel.Exceptions;
using PantryCloud.SharedKernel.Extensions;
using Microsoft.Extensions.Configuration;

namespace PantryCloud.Notification.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddPresentationLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen();
        services.AddEndpointsApiExplorer();
        services.AddControllers();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}

