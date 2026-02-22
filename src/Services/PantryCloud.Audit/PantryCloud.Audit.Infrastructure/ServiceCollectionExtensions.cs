using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Audit.Application;
using PantryCloud.Audit.Core;
using PantryCloud.Audit.Core.Options;
using PantryCloud.Audit.Infrastructure.BackgroundServices;
using PantryCloud.Audit.Infrastructure.Persistence;
using PantryCloud.Audit.Infrastructure.Services;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Extensions;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.SharedKernel.Observability;
using PantryCloud.SharedKernel.Persistence;

namespace PantryCloud.Audit.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiConfiguration = new ApiConfiguration();
        configuration.Bind(apiConfiguration);
        services.AddSingleton(apiConfiguration);

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContextWithAuditing<AuditDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddMessaging(configuration, typeof(ServiceCollectionExtensions).Assembly);
        services.AddHealthChecks().AddNpgSql(connectionString!);
        services.AddJwtBearerFromConfiguration(configuration, "App:IdentityUrl");
        services.AddAuthorization();
        services.AddCorrelationId();

        services.Configure<AuditOptions>(configuration.GetSection(AuditOptions.SectionName));

        services.AddScoped<IHouseholdMembershipRepository, HouseholdMembershipRepository>();
        services.AddScoped<IAuditQueryService, AuditQueryService>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddHostedService<AuditRetentionBackgroundService>();

        services.AddPrometheusMetrics();
        
        return services;
    }
}
