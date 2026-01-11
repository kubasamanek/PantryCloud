using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PantryCloud.Household.Application;
using PantryCloud.Household.Core;
using PantryCloud.Household.Infrastructure.Persistence;
using PantryCloud.Household.Infrastructure.Services;

namespace PantryCloud.Household.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiConfiguration = new ApiConfiguration();
        configuration.Bind(apiConfiguration);
        services.AddSingleton(apiConfiguration);
        
        services.AddDbContext<HouseholdDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var identityUrl = apiConfiguration.App.IdentityUrl;
                options.Audience = "PantryCloud.WebClient";
                options.MetadataAddress = $"{identityUrl}/.well-known/openid-configuration";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = apiConfiguration.Jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = apiConfiguration.Jwt.Audience,
                    ValidateLifetime = true,
                };
                options.RequireHttpsMetadata = false;
            });

        services.AddAuthorization();

        services.AddHttpContextAccessor();
        
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IHouseholdManagementService, HouseholdManagementService>();
        services.AddScoped<IInvitationService, InvitationService>();
        
        return services;
    }
}