using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using PantryCloud.Notification.Application.Queries;
using PantryCloud.SharedKernel.Behaviors;

namespace PantryCloud.Notification.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => {}, typeof(GetMyNotificationsQuery));
        services.AddValidatorsFromAssemblyContaining<GetMyNotificationsQuery>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(GetMyNotificationsQuery).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}

