using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Identity.Application.Commands;
using PantryCloud.SharedKernel.Behaviors;

namespace PantryCloud.Identity.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));

        services.AddValidatorsFromAssembly(typeof(RegisterCommand).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}