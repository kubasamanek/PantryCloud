using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PantryCloud.ApiGateway.Core;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

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

        services.AddSingleton<IProxyConfigProvider>(_ => 
            new InMemoryConfigProvider(
                GetRoutes(apiConfiguration.Services),
                GetClusters(apiConfiguration.Services)));

        services.AddReverseProxy()
            .AddTransforms(builderContext =>
            {
                builderContext.AddRequestTransform(async transformContext =>
                {
                    var user = transformContext.HttpContext.User;
                    
                    if (user.Identity?.IsAuthenticated == true)
                    {
                        // Extract user ID (from 'sub' claim)
                        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) 
                                     ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
                        if (!string.IsNullOrEmpty(userId))
                        {
                            transformContext.ProxyRequest.Headers.Add("X-User-Id", userId);
                        }

                        // Extract email
                        var email = user.FindFirstValue(ClaimTypes.Email) 
                                   ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email);
                        if (!string.IsNullOrEmpty(email))
                        {
                            transformContext.ProxyRequest.Headers.Add("X-User-Email", email);
                        }

                        // Extract email verified status
                        var emailVerified = user.FindFirstValue("email_verified");
                        if (!string.IsNullOrEmpty(emailVerified))
                        {
                            transformContext.ProxyRequest.Headers.Add("X-User-Email-Verified", emailVerified);
                        }
                    }

                    await ValueTask.CompletedTask;
                });
            });

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
