using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PantryCloud.SharedKernel.Correlation;

namespace PantryCloud.ApiGateway.Infrastructure.Middleware;

/// <summary>
/// Provides high-level traffic observability for the API Gateway.
/// Logs incoming requests and their completion status, including execution duration, 
/// user identity, and correlation context. This is essential for monitoring system health and debugging latency issues.
/// </summary>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items[CorrelationIdConstants.HttpContextItemKey]?.ToString() ?? CorrelationIdConstants.UnknownPlaceholder;
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString;
        var userAgent = context.Request.Headers[Constants.UserAgentHeader].FirstOrDefault() ?? Constants.UnknownPlaceholder;
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? Constants.UnknownPlaceholder;
        var user = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous"
            : "anonymous";

        logger.LogInformation(
            "Incoming request: {Method} {Path}{QueryString} from {RemoteIp} (User: {User}, CorrelationId: {CorrelationId}, UserAgent: {UserAgent})",
            method, path, queryString, remoteIp, user, correlationId, userAgent);

        try
        {
            await next(context);
            stopwatch.Stop();

            logger.LogInformation(
                "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms (CorrelationId: {CorrelationId})",
                method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds, correlationId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex,
                "Request failed: {Method} {Path} - Duration: {Duration}ms - Error: {ErrorMessage} (CorrelationId: {CorrelationId})",
                method, path, stopwatch.ElapsedMilliseconds, ex.Message, correlationId);
            throw;
        }
    }
}

