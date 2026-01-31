namespace PantryCloud.SharedKernel.Correlation;

/// <summary>
/// Provides access to the current request's correlation ID.
/// </summary>
public interface ICorrelationIdProvider
{
    /// <summary>
    /// Gets the current correlation ID, or null if not available.
    /// </summary>
    string? GetCorrelationId();

    /// <summary>
    /// Gets the current correlation ID, or a placeholder if not available.
    /// </summary>
    string GetCorrelationIdOrPlaceholder() => GetCorrelationId() ?? CorrelationIdConstants.UnknownPlaceholder;
}
