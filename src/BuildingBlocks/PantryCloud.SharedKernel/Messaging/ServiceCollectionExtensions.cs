using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PantryCloud.SharedKernel.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] consumerAssemblies)
    {
        var rabbitMqHost = configuration["Messaging:RabbitMQ:Host"] ?? "rabbitmq";
        var rabbitMqUser = configuration["Messaging:RabbitMQ:Username"] ?? "admin";
        var rabbitMqPass = configuration["Messaging:RabbitMQ:Password"] ?? "password";

        services.AddMassTransit(busConfig =>
        {
            busConfig.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(false));
            
            if (consumerAssemblies.Length > 0)
            {
                foreach (var assembly in consumerAssemblies)
                {
                    busConfig.AddConsumers(assembly);
                }
            }
            else
            {
                var callingAssembly = Assembly.GetCallingAssembly();
                busConfig.AddConsumers(callingAssembly);
            }

            busConfig.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqHost, "/", h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPass);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IMessageBus, MassTransitBus>();

        return services;
    }
}

