using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Core;
using PantryCloud.Recipe.Infrastructure.Persistence;
using PantryCloud.Recipe.Infrastructure.Services;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Identity;

namespace PantryCloud.Recipe.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiConfiguration = new ApiConfiguration();
        configuration.Bind(apiConfiguration);
        services.AddSingleton(apiConfiguration);

        // MongoDB configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("MongoDB connection string is required.");

        var client = new MongoClient(connectionString);
        var databaseName = apiConfiguration.MongoDb.DatabaseName;
        var database = client.GetDatabase(databaseName);

        // Register MongoDB services
        services.AddSingleton(database);
        services.AddScoped<RecipeDbContext>();

        // Add DB Health Check
        services.AddHealthChecks()
            .AddCheck("mongodb", () =>
            {
                try
                {
                    client.GetDatabase(databaseName).RunCommand<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
                }
                catch
                {
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy();
                }
            });

        // Add Authentication
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

        // Register repositories and services
        services.AddScoped<IRecipeRepository, MongoRecipeRepository>();
        services.AddScoped<IRecipeSearchService, LocalMongoRecipeSearchService>();

        return services;
    }
}

