using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.ApiGateway.Core;

namespace PantryCloud.ApiGateway.Presentation.Extensions;

public static class RateLimitingExtensions
{
    /// <summary>
    /// Configures rate limiting policies to protect downstream services from excessive traffic and DoS attacks.
    /// </summary>
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        ApiConfiguration apiConfiguration)
    {
        if (apiConfiguration.Gateway.RateLimit.Enabled)
        {
            services.AddRateLimiter(options =>
            {
                // Define the Global Limiter applied to all requests
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.IsAuthenticated == true
                            ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous"
                            : context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = apiConfiguration.Gateway.RateLimit.PermitLimit,
                            Window = TimeSpan.FromSeconds(apiConfiguration.Gateway.RateLimit.WindowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = apiConfiguration.Gateway.RateLimit.QueueLimit
                        }));

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new { error = "Rate limit exceeded. Please try again later." },
                        cancellationToken);
                };
            });
        }

        return services;
    }
}


