using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PantryCloud.ApiGateway.Infrastructure.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString;
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var user = context.User.Identity?.IsAuthenticated == true 
            ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous"
            : "anonymous";

        _logger.LogInformation(
            "Incoming request: {Method} {Path}{QueryString} from {RemoteIp} (User: {User}, CorrelationId: {CorrelationId}, UserAgent: {UserAgent})",
            method, path, queryString, remoteIp, user, correlationId, userAgent);

        try
        {
            await _next(context);
            stopwatch.Stop();

            _logger.LogInformation(
                "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms (CorrelationId: {CorrelationId})",
                method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds, correlationId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Request failed: {Method} {Path} - Duration: {Duration}ms - Error: {ErrorMessage} (CorrelationId: {CorrelationId})",
                method, path, stopwatch.ElapsedMilliseconds, ex.Message, correlationId);
            throw;
        }
    }
}

