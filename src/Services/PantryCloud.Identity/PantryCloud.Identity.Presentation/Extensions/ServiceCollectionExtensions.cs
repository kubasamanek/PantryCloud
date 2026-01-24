using PantryCloud.Identity.Presentation.Mapping;
using PantryCloud.SharedKernel.Exceptions;
using PantryCloud.SharedKernel.Extensions;
using PantryCloud.SharedKernel.Logging;

namespace PantryCloud.Identity.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddPresentationLayerServices(this IServiceCollection services,  IConfiguration configuration)
    { 
        services.AddSerilogLogging(configuration);
        
        services.AddSwaggerGenWithAuth();
        services.AddEndpointsApiExplorer();
        
        services.AddControllers();
        
        services.AddAutoMapper(_ => { }, typeof(AuthMappingProfile));
        
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        return services;
    }
}