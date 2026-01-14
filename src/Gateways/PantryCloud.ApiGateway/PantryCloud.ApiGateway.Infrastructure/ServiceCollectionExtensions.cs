using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PantryCloud.ApiGateway.Core;
using PantryCloud.ApiGateway.Infrastructure.Transforms;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;

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
                policyRegistry.Add(Constants.RetryPolicyName, retryPolicy);
            }
            if (resilienceSettings.CircuitBreaker.Enabled)
            {
                policyRegistry.Add(Constants.CircuitBreakerPolicyName, circuitBreakerPolicy);
            }
        }

        services.AddSingleton<IProxyConfigProvider>(_ => 
            new InMemoryConfigProvider(
                GetRoutes(apiConfiguration.Services),
                GetClusters(apiConfiguration.Services, resilienceSettings)));

        services.AddSingleton<IForwarderHttpClientFactory, ResilientForwarderHttpClientFactory>();

        services.AddReverseProxy()
            .AddTransforms<CorrelationIdTransformProvider>();

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
        // Set request timeout
        var forwarderRequestConfig = new ForwarderRequestConfig
        {
            ActivityTimeout = TimeSpan.FromSeconds(resilienceSettings.RequestTimeoutSeconds)
        };

        // Add policies to metadata
        var metadata = new Dictionary<string, string>();
        if (resilienceSettings.Retry.Enabled)
        {
            metadata[Constants.RetryPolicyName] = Constants.RetryPolicyName;
        }
        if (resilienceSettings.CircuitBreaker.Enabled)
        {
            metadata[Constants.CircuitBreakerPolicyName] = Constants.CircuitBreakerPolicyName;
        }

        return
        [
            CreateClusterConfig(RouteConfiguration.IdentityClusterId, services.IdentityService),
            CreateClusterConfig(RouteConfiguration.HouseholdClusterId, services.HouseholdService),
            //CreateClusterConfig(RouteConfiguration.PantryClusterId, services.PantryService),
            //CreateClusterConfig(RouteConfiguration.RecipeClusterId, services.RecipeService),
            //CreateClusterConfig(RouteConfiguration.ShoppingListClusterId, services.ShoppingListService)
        ];

        ClusterConfig CreateClusterConfig(string clusterId, string serviceAddress)
        {
            var cluster = new ClusterConfig
            {
                ClusterId = clusterId,
                HttpRequest = forwarderRequestConfig,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["default"] = new()
                    {
                        Address = serviceAddress
                    }
                },
                Metadata = metadata.Count > 0 ? metadata : null,
                HealthCheck = resilienceSettings.HealthCheck.Enabled
                    ? new HealthCheckConfig
                    {
                        Active = new ActiveHealthCheckConfig
                        {
                            Enabled = true,
                            Interval = TimeSpan.FromSeconds(resilienceSettings.HealthCheck.IntervalSeconds),
                            Timeout = TimeSpan.FromSeconds(resilienceSettings.HealthCheck.TimeoutSeconds),
                            Path = resilienceSettings.HealthCheck.Path,
                            Policy = resilienceSettings.HealthCheck.Policy
                        }
                    }
                    : null
            };

            return cluster;
        }
    }
}
