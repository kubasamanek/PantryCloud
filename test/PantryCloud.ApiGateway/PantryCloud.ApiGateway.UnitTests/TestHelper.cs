using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace PantryCloud.ApiGateway.UnitTests;

internal static class TestHelper
{
    public static HttpContext CreateHttpContext(
        string? correlationId = null,
        string? method = Constants.Http.MethodGet,
        string? path = Constants.Http.PathApiTest,
        bool isAuthenticated = false,
        string? userId = null)
    {
        var context = new DefaultHttpContext
        {
            Request =
            {
                Method = method ?? Constants.Http.MethodGet,
                Path = path ?? Constants.Http.PathApiTest
            }
        };

        if (correlationId != null)
        {
            context.Request.Headers[Constants.Correlation.HeaderName] = correlationId;
        }

        var services = new ServiceCollection();
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
    
    public static ILogger<T> MockLogger<T>() where T : class
        => Substitute.For<ILogger<T>>();

}
