using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Extensions;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.SharedKernel.Observability;
using PantryCloud.SharedKernel.Persistence;
using PantryCloud.ShoppingList.Application;
using PantryCloud.ShoppingList.Core;
using PantryCloud.ShoppingList.Infrastructure.Persistence;
using PantryCloud.ShoppingList.Infrastructure.Services;

namespace PantryCloud.ShoppingList.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiConfiguration = new ApiConfiguration();
        configuration.Bind(apiConfiguration);
        services.AddSingleton(apiConfiguration);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextWithAuditing<ShoppingListDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ShoppingListDbContext>());

        services.AddOutboxRelay<ShoppingListDbContext>();

        services.AddMessaging(configuration, typeof(ServiceCollectionExtensions).Assembly);

        services.AddHealthChecks().AddNpgSql(connectionString!);

        services.AddJwtBearerFromConfiguration(configuration, "App:IdentityUrl");

        services.AddAuthorization();

        services.AddCorrelationId();

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IShoppingListManagementService, ShoppingListManagementService>();

        services.AddOpenTelemetryTracing(configuration);
        services.AddApiVersioningDefaults();
        
        return services;
    }
}


