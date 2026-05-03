using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using PantryCloud.SharedKernel.Correlation;

namespace PantryCloud.SharedKernel.Http;

/// <summary>
/// Extension methods for HttpClient to forward correlation ID in outbound HTTP requests.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Adds correlation ID header to the HTTP request from the current HttpContext.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public static void AddCorrelationIdHeader(this HttpRequestMessage request, IHttpContextAccessor httpContextAccessor)
    {
        var correlationId = httpContextAccessor.GetCorrelationIdFromContext();
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.Add(CorrelationIdConstants.HeaderName, correlationId);
        }
    }

    /// <summary>
    /// Adds correlation ID header to the HTTP request from the provided correlation ID provider.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="correlationIdProvider">The correlation ID provider.</param>
    public static void AddCorrelationIdHeader(this HttpRequestMessage request, ICorrelationIdProvider correlationIdProvider)
    {
        var correlationId = correlationIdProvider.GetCorrelationId();
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.Add(CorrelationIdConstants.HeaderName, correlationId);
        }
    }

    /// <summary>
    /// Gets the correlation ID from the current HttpContext.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <returns>The correlation ID if available, otherwise null.</returns>
    public static string? GetCorrelationIdFromContext(this IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // First try HttpContext.Items (set by middleware)
        if (httpContext.Items.TryGetValue(CorrelationIdConstants.HttpContextItemKey, out var item) && item is string correlationId)
            return correlationId;

        // Fallback to header
        if (httpContext.Request.Headers.TryGetValue(CorrelationIdConstants.HeaderName, out var headerValues))
        {
            return headerValues.FirstOrDefault();
        }

        return null;
    }
}
