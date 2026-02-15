using PantryCloud.SharedKernel.Exceptions;
using PantryCloud.SharedKernel.Extensions;

namespace PantryCloud.Pantry.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddPresentationLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGenWithAuth();

        services.AddEndpointsApiExplorer();

        services.AddControllers();

        services.AddAutoMapper(_ => { }, typeof(Program));

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}

