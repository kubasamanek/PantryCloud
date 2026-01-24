namespace PantryCloud.SharedKernel.Correlation;

/// <summary>
/// Constants for correlation ID handling across services.
/// </summary>
public static class CorrelationIdConstants
{
    /// <summary>
    /// HTTP header name for correlation ID.
    /// </summary>
    public const string HeaderName = "X-Correlation-Id";

    /// <summary>
    /// HttpContext.Items key for storing correlation ID.
    /// This is the key that Serilog.Enrichers.CorrelationId reads from.
    /// </summary>
    public const string HttpContextItemKey = "CorrelationId";

    /// <summary>
    /// Placeholder value when correlation ID is not available.
    /// </summary>
    public const string UnknownPlaceholder = "unknown";
}

