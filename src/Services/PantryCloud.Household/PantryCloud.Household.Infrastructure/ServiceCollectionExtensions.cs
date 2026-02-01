using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Household.Application;
using PantryCloud.Household.Application.Commands;
using PantryCloud.Household.Core;
using PantryCloud.Household.Infrastructure.Persistence;
using PantryCloud.Household.Infrastructure.Services;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Extensions;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.SharedKernel.Persistence;

namespace PantryCloud.Household.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiConfiguration = new ApiConfiguration();
        configuration.Bind(apiConfiguration);
        services.AddSingleton(apiConfiguration);
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextWithAuditing<HouseholdDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddMessaging(configuration, typeof(CreateHouseholdCommand).Assembly);
        
        services.AddHealthChecks().AddNpgSql(connectionString!);

        services.AddJwtBearerFromConfiguration(configuration, "App:IdentityUrl");

        services.AddAuthorization();

        services.AddCorrelationId();
        
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IHouseholdManagementService, HouseholdManagementService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IPreferencesService, PreferencesService>();
        
        return services;
    }
}