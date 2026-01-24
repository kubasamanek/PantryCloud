using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Extension methods for registering messaging services using MassTransit and RabbitMQ.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MassTransit with RabbitMQ and registers message consumers from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing RabbitMQ connection settings.</param>
    /// <param name="consumerAssemblies">Assemblies to scan for consumers. If none provided, scans the calling assembly.</param>
    /// <returns>The service collection for chaining.</returns>
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
            busConfig.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(true));
            
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

