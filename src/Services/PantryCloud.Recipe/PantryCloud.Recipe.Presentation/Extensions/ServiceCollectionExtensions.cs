using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Recipe.Presentation.Validators;
using PantryCloud.SharedKernel.Exceptions;
using PantryCloud.SharedKernel.Extensions;

namespace PantryCloud.Recipe.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddPresentationLayerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSwaggerGenWithAuth();
        services.AddEndpointsApiExplorer();
        services.AddControllers();

        services.AddAutoMapper(_ => { }, typeof(Program));
        services.AddValidatorsFromAssemblyContaining<SearchRecipesRequestValidator>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}