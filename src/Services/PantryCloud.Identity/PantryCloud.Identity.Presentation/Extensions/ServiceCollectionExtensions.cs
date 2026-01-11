using PantryCloud.Identity.Presentation.Mapping;
using PantryCloud.SharedKernel.Exceptions;
using PantryCloud.SharedKernel.Extensions;

namespace PantryCloud.Identity.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddPresentationLayerServices(this IServiceCollection services)
    {
        services.AddSwaggerGenWithAuth();
        services.AddEndpointsApiExplorer();
        
        services.AddControllers();
        
        services.AddAutoMapper(_ => { }, typeof(AuthMappingProfile));
        
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        return services;
    }
}