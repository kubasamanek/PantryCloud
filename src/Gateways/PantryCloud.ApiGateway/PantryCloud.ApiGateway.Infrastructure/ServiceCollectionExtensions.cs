using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.ApiGateway.Core;
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

        services.AddSingleton<IProxyConfigProvider>(_ => 
            new InMemoryConfigProvider(
                GetRoutes(apiConfiguration.Services),
                GetClusters(apiConfiguration.Services)));

        services.AddReverseProxy();

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

    private static ClusterConfig[] GetClusters(ServiceEndpoints services)
    {
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
