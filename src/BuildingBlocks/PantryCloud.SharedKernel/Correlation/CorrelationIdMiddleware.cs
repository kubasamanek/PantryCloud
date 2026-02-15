using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace PantryCloud.SharedKernel.Correlation;

/// <summary>
/// Middleware that extracts correlation ID from incoming request headers and establishes it in the request context.
/// If no correlation ID is present, generates a new one (typically at the Gateway/entry point).
/// Works with Serilog to automatically include correlation ID in all logs via LogContext.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headerValue = context.Request.Headers[CorrelationIdConstants.HeaderName];

        var correlationId = headerValue.ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Store in HttpContext.Items for access throughout the request pipeline
        context.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId;

        // Set in request headers (for YARP/proxy forwarding) and response headers
        context.Request.Headers[CorrelationIdConstants.HeaderName] = correlationId;
        context.Response.Headers[CorrelationIdConstants.HeaderName] = correlationId;

        // Enrich logging with Correlation ID
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
