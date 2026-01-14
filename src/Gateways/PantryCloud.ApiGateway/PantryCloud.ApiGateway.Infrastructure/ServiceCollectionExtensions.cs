using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PantryCloud.ApiGateway.Core;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using Yarp.ReverseProxy.Configuration;

namespace PantryCloud.ApiGateway.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiConfiguration = new ApiConfiguration();
        configuration.Bind(apiConfiguration);
        services.AddSingleton(apiConfiguration);

        // Add JWT Authentication
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

        // Add Resilience Policies (Polly)
        // Note: These policies are registered for potential use with custom HttpClient instances.
        // YARP manages its own HttpClient instances, so timeouts are configured via ClusterConfig.HttpClient.RequestTimeout.
        // For full Polly integration with YARP, consider using a custom IForwarderHttpClientFactory.
        var resilienceSettings = apiConfiguration.Gateway.Resilience;
        
        if (resilienceSettings.Retry.Enabled || resilienceSettings.CircuitBreaker.Enabled)
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: resilienceSettings.Retry.Enabled ? resilienceSettings.Retry.MaxRetryAttempts : 0,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(
                        resilienceSettings.Retry.BaseDelayMilliseconds * Math.Pow(2, retryAttempt)));

            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: resilienceSettings.CircuitBreaker.Enabled 
                        ? resilienceSettings.CircuitBreaker.FailureThreshold 
                        : int.MaxValue,
                    durationOfBreak: TimeSpan.FromSeconds(resilienceSettings.CircuitBreaker.DurationOfBreakSeconds));

            var policyRegistry = new PolicyRegistry();
            services.AddSingleton<IReadOnlyPolicyRegistry<string>>(policyRegistry);
            if (resilienceSettings.Retry.Enabled)
            {
                policyRegistry.Add("RetryPolicy", retryPolicy);
            }
            if (resilienceSettings.CircuitBreaker.Enabled)
            {
                policyRegistry.Add("CircuitBreakerPolicy", circuitBreakerPolicy);
            }
        }

        services.AddSingleton<IProxyConfigProvider>(_ => 
            new InMemoryConfigProvider(
                GetRoutes(apiConfiguration.Services),
                GetClusters(apiConfiguration.Services, resilienceSettings)));

        services.AddReverseProxy()
            .AddTransforms<Transforms.CorrelationIdTransformProvider>();

        return services;
    }

    private static RouteConfig[] GetRoutes(ServiceEndpoints services)
    {
        return
        [
            new RouteConfig
            {
                RouteId = RouteConfiguration.IdentityRouteId,
                ClusterId = RouteConfiguration.IdentityClusterId,
                Match = new RouteMatch
                {
                    Path = "/api/identity/{**catch-all}"
                },
                Transforms =
                [
                    new Dictionary<string, string>
                    {
                        ["PathPattern"] = "/{**catch-all}"
                    }
                ]
            },
            new RouteConfig
            {
                RouteId = RouteConfiguration.HouseholdRouteId,
                ClusterId = RouteConfiguration.HouseholdClusterId,
                Match = new RouteMatch
                {
                    Path = "/api/household/{**catch-all}"
                },
                Transforms =
                [
                    new Dictionary<string, string>
                    {
                        ["PathPattern"] = "/{**catch-all}"
                    }
                ]
            },
            new RouteConfig
            {
                RouteId = RouteConfiguration.PantryRouteId,
                ClusterId = RouteConfiguration.PantryClusterId,
                Match = new RouteMatch
                {
                    Path = "/api/pantry/{**catch-all}"
                },
                Transforms =
                [
                    new Dictionary<string, string>
                    {
                        ["PathPattern"] = "/{**catch-all}"
                    }
                ]
            },
            new RouteConfig
            {
                RouteId = RouteConfiguration.RecipeRouteId,
                ClusterId = RouteConfiguration.RecipeClusterId,
                Match = new RouteMatch
                {
                    Path = "/api/recipe/{**catch-all}"
                },
                Transforms =
                [
                    new Dictionary<string, string>
                    {
                        ["PathPattern"] = "/{**catch-all}"
                    }
                ]
            },
            new RouteConfig
            {
                RouteId = RouteConfiguration.ShoppingListRouteId,
                ClusterId = RouteConfiguration.ShoppingListClusterId,
                Match = new RouteMatch
                {
                    Path = "/api/shoppinglist/{**catch-all}"
                },
                Transforms =
                [
                    new Dictionary<string, string>
                    {
                        ["PathPattern"] = "/{**catch-all}"
                    }
                ]
            }
        ];
    }

    private static ClusterConfig[] GetClusters(ServiceEndpoints services, ResilienceSettings resilienceSettings)
    {
        // Note: Request timeout configuration can be added via IForwarderHttpClientFactory if needed
        // For now, using default HttpClient timeout settings
        
        return
        [
            new ClusterConfig
            {
                ClusterId = RouteConfiguration.IdentityClusterId,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["default"] = new DestinationConfig
                    {
                        Address = services.IdentityService
                    }
                }
            },
            new ClusterConfig
            {
                ClusterId = RouteConfiguration.HouseholdClusterId,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["default"] = new DestinationConfig
                    {
                        Address = services.HouseholdService
                    }
                }
            },
            new ClusterConfig
            {
                ClusterId = RouteConfiguration.PantryClusterId,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["default"] = new DestinationConfig
                    {
                        Address = services.PantryService
                    }
                }
            },
            new ClusterConfig
            {
                ClusterId = RouteConfiguration.RecipeClusterId,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["default"] = new DestinationConfig
                    {
                        Address = services.RecipeService
                    }
                }
            },
            new ClusterConfig
            {
                ClusterId = RouteConfiguration.ShoppingListClusterId,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["default"] = new DestinationConfig
                    {
                        Address = services.ShoppingListService
                    }
                }
            }
        ];
    }
}
