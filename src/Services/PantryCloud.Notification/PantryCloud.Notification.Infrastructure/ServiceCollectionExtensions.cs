using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Application.Consumers;
using PantryCloud.Notification.Infrastructure.Services;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR();
        services.AddMessaging(configuration, typeof(MemberLeftHouseholdConsumer).Assembly);

        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}

