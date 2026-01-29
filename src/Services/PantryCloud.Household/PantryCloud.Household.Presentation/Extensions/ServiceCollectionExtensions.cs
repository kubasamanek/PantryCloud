using PantryCloud.SharedKernel.Exceptions;
using PantryCloud.SharedKernel.Extensions;
using PantryCloud.SharedKernel.Logging;

namespace PantryCloud.Household.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddPresentationLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilogLogging(configuration);

        services.AddSwaggerGenWithAuth();

        services.AddEndpointsApiExplorer();
        
        services.AddControllers();
        
        services.AddAutoMapper(_ => {}, typeof(Program));
        
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        return services;
    }
}