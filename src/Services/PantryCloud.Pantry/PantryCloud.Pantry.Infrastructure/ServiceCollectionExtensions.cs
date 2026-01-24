using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PantryCloud.Pantry.Application;
using PantryCloud.Pantry.Core;
using PantryCloud.Pantry.Infrastructure.Persistence;
using PantryCloud.Pantry.Infrastructure.Services;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Messaging;
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
        
        services.AddMessaging(configuration, typeof(ServiceCollectionExtensions).Assembly);
        
        services.AddHealthChecks().AddNpgSql(connectionString!);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var identityUrl = apiConfiguration.App.IdentityUrl;
                options.Audience = apiConfiguration.Jwt.Audience;
                options.MetadataAddress = $"{identityUrl}/.well-known/openid-configuration";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = apiConfiguration.Jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = apiConfiguration.Jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };
                options.RequireHttpsMetadata = false;
            });

        services.AddAuthorization();

        services.AddCorrelationId();
        
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IPantryManagementService, PantryManagementService>();
        services.AddScoped<IHouseholdCacheHydrationService, HouseholdCacheHydrationService>();
        
        services.AddHttpClient<HouseholdCacheHydrationService>();
        
        return services;
    }
}

