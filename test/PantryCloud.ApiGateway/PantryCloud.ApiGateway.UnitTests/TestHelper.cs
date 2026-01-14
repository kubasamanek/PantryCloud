using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.ApiGateway.Infrastructure.Middleware;

namespace PantryCloud.ApiGateway.UnitTests;

internal static class TestHelper
{
    public static ILogger<T> MockLogger<T>() where T : class
        => Substitute.For<ILogger<T>>();

    public static HttpContext CreateHttpContext(
        string? correlationId = null,
        string? method = "GET",
        string? path = "/api/test",
        bool isAuthenticated = false,
        string? userId = null)
    {
        var context = new DefaultHttpContext
        {
            Request =
            {
                Method = method ?? "GET",
                Path = path ?? "/api/test"
            }
        };

        if (correlationId != null)
        {
            context.Request.Headers["X-Correlation-Id"] = correlationId;
        }

        var services = new ServiceCollection();
        services.AddSingleton(MockLogger<CorrelationIdMiddleware>());
        context.RequestServices = services.BuildServiceProvider();

        if (!isAuthenticated || userId == null) return context;
        
        var identity = new System.Security.Claims.ClaimsIdentity("test");
        identity.AddClaim(new System.Security.Claims.Claim(
            System.Security.Claims.ClaimTypes.NameIdentifier, userId));
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);

        return context;
    }

    public static RequestDelegate CreateMockNext(Action<HttpContext>? callback = null)
    {
        return async context =>
        {
            callback?.Invoke(context);
            await Task.CompletedTask;
        };
    }
}
