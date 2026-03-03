using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Pantry.Application;
using PantryCloud.Pantry.Core;
using PantryCloud.Pantry.Core.Options;
using PantryCloud.Pantry.Infrastructure.BackgroundServices;
using PantryCloud.Pantry.Infrastructure.Persistence;
using PantryCloud.Pantry.Infrastructure.Services;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Extensions;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.SharedKernel.Observability;
using PantryCloud.SharedKernel.Persistence;

namespace PantryCloud.Pantry.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiConfiguration = new ApiConfiguration();
        configuration.Bind(apiConfiguration);
        services.AddSingleton(apiConfiguration);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextWithAuditing<PantryDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<PantryDbContext>());

        services.AddOutboxRelay<PantryDbContext>();

        services.AddMessaging(configuration, typeof(ServiceCollectionExtensions).Assembly);

        services.AddHealthChecks().AddNpgSql(connectionString!);

        services.AddJwtBearerFromConfiguration(configuration, "App:IdentityUrl");

        services.AddAuthorization();

        services.AddCorrelationId();

        services.Configure<ExpirationCheckOptions>(
            configuration.GetSection(ExpirationCheckOptions.SectionName));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IExpirationCheckService, ExpirationCheckService>();
        services.AddHostedService<ExpirationCheckBackgroundService>();

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IPantryManagementService, PantryManagementService>();

        services.AddOpenTelemetryTracing(configuration);
        services.AddApiVersioningDefaults();

        return services;
    }
}

