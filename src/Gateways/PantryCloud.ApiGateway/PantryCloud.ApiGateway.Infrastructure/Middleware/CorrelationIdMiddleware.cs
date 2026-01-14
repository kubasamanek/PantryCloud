using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PantryCloud.ApiGateway.Infrastructure.Middleware;

/// <summary>
/// Establishes a unique tracking identifier (Correlation ID) for the current request lifecycle.
/// Ensures that all logs generated during the processing of this request are tagged with the same ID,
/// enabling effective distributed tracing across the system.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[Constants.CorrelationIdHeader].FirstOrDefault()
                           ?? Guid.NewGuid().ToString();

        context.Request.Headers[Constants.CorrelationIdHeader] = correlationId;
        context.Response.Headers[Constants.CorrelationIdHeader] = correlationId;
        context.Items[Constants.CorrelationIdItem] = correlationId;

        using (context.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>()
            .BeginScope(new Dictionary<string, object> { [Constants.CorrelationIdItem] = correlationId }))
        {
            await next(context);
        }
    }
}

