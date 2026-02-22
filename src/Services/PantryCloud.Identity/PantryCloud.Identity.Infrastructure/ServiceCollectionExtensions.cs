using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Identity.Application;
using PantryCloud.Identity.Core;
using PantryCloud.Identity.Infrastructure.Persistence;
using PantryCloud.Identity.Infrastructure.Services;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Observability;

namespace PantryCloud.Identity.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiConfiguration = new ApiConfiguration();
        configuration.Bind(apiConfiguration);
        services.AddSingleton(apiConfiguration);

        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddIdentityJwtAuth(apiConfiguration);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddCorrelationId();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHealthChecks().AddNpgSql(connectionString!);
        services.AddPrometheusMetrics();
        
        return services;
    }
}