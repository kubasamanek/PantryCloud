using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Infrastructure.Consumers;
using PantryCloud.Notification.Core;
using PantryCloud.Notification.Infrastructure.Persistence;
using PantryCloud.Notification.Infrastructure.Services;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Extensions;
using PantryCloud.SharedKernel.Messaging;
using StackExchange.Redis;

namespace PantryCloud.Notification.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiConfiguration = new ApiConfiguration();
        configuration.Bind(apiConfiguration);
        services.AddSingleton(apiConfiguration);

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<NotificationDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        services.AddHealthChecks().AddNpgSql(connectionString!);

        services.AddJwtBearerFromConfiguration(configuration, "App:IdentityUrl", options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        context.Token = accessToken;
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        services.AddCorrelationId();

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        var redis = ConnectionMultiplexer.Connect(redisConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(redis);

        services.AddSignalR().AddStackExchangeRedis(redisConnectionString, options =>
        {
            options.Configuration.ChannelPrefix = RedisChannel.Literal("PantryCloud:SignalR:");
        });

        services.AddDataProtection()
            .SetApplicationName("PantryCloud")
            .PersistKeysToStackExchangeRedis(redis, "PantryCloud:DataProtection:Keys");

        services.AddMessaging(configuration, typeof(MemberLeftHouseholdConsumer).Assembly);

        services.AddScoped<IHouseholdMembershipRepository, HouseholdMembershipRepository>();
        services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}

