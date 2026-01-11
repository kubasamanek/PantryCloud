using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Household.Application;
using PantryCloud.Household.Infrastructure.Persistence;
using PantryCloud.Household.Infrastructure.Services;

namespace PantryCloud.Household.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HouseholdDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpContextAccessor();
        
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IHouseholdManagementService, HouseholdManagementService>();
        services.AddScoped<IInvitationService, InvitationService>();
        
        return services;
    }
}