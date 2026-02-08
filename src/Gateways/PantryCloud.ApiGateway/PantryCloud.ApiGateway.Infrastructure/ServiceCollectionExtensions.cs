using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PantryCloud.ApiGateway.Core;
using PantryCloud.SharedKernel.Extensions;
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

        services.AddJwtBearerFromConfiguration(configuration, "App:IdentityUrl");

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddAuthorization();

        var resilienceSettings = apiConfiguration.Gateway.Resilience;
        var policyRegistry = new PolicyRegistry();
        
        if (resilienceSettings.Retry.Enabled || resilienceSettings.CircuitBreaker.Enabled)
        {
            // Create policies per service to isolate failures
            var serviceNames = new[] { "Identity", "Household", "Pantry", "Recipe", "ShoppingList", "Notification", "Audit" };
            
            foreach (var service in serviceNames)
            {
                // Create service-specific retry policy
                if (resilienceSettings.Retry.Enabled)
                {
                    var retryPolicy = HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(
                            retryCount: resilienceSettings.Retry.MaxRetryAttempts,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(
                                resilienceSettings.Retry.BaseDelayMilliseconds * Math.Pow(2, retryAttempt)));
                    
                    policyRegistry.Add($"{Constants.RetryPolicyName}-{service}", retryPolicy);
                }

                // Create service-specific circuit breaker policy
                if (resilienceSettings.CircuitBreaker.Enabled)
                {
                    var circuitBreakerPolicy = HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .CircuitBreakerAsync(
                            handledEventsAllowedBeforeBreaking: resilienceSettings.CircuitBreaker.FailureThreshold,
                            durationOfBreak: TimeSpan.FromSeconds(resilienceSettings.CircuitBreaker.DurationOfBreakSeconds));
                    
                    policyRegistry.Add($"{Constants.CircuitBreakerPolicyName}-{service}", circuitBreakerPolicy);
                }
            }
        }
        
        services.AddSingleton<IReadOnlyPolicyRegistry<string>>(policyRegistry);

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
                AuthorizationPolicy = "Anonymous",
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
            },
            new RouteConfig
            {
                RouteId = RouteConfiguration.NotificationRouteId,
                ClusterId = RouteConfiguration.NotificationClusterId,
                Match = new RouteMatch
                {
                    Path = "/api/notification/{**catch-all}"
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
                RouteId = RouteConfiguration.AuditRouteId,
                ClusterId = RouteConfiguration.AuditClusterId,
                Match = new RouteMatch
                {
                    Path = "/api/audit/{**catch-all}"
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

        return
        [
            CreateClusterConfig(RouteConfiguration.IdentityClusterId, services.IdentityService, "Identity"),
            CreateClusterConfig(RouteConfiguration.HouseholdClusterId, services.HouseholdService, "Household"),
            CreateClusterConfig(RouteConfiguration.PantryClusterId, services.PantryService, "Pantry"),
            CreateClusterConfig(RouteConfiguration.RecipeClusterId, services.RecipeService, "Recipe"),
            CreateClusterConfig(RouteConfiguration.ShoppingListClusterId, services.ShoppingListService, "ShoppingList"),
            CreateClusterConfig(RouteConfiguration.NotificationClusterId, services.NotificationService, "Notification"),
            CreateClusterConfig(RouteConfiguration.AuditClusterId, services.AuditService, "Audit")
        ];

        ClusterConfig CreateClusterConfig(string clusterId, string serviceAddress, string serviceName)
        {
            // Create service-specific metadata with isolated policies
            var metadata = new Dictionary<string, string>();
            if (resilienceSettings.Retry.Enabled)
            {
                metadata[Constants.RetryPolicyName] = $"{Constants.RetryPolicyName}-{serviceName}";
            }
            if (resilienceSettings.CircuitBreaker.Enabled)
            {
                metadata[Constants.CircuitBreakerPolicyName] = $"{Constants.CircuitBreakerPolicyName}-{serviceName}";
            }
            
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
