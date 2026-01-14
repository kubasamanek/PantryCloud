namespace PantryCloud.ApiGateway.Infrastructure;

internal static class Constants
{
    internal const string CorrelationIdHeader = "X-Correlation-Id";
    internal const string UserAgentHeader = "X-User-Agent";
    internal const string CorrelationIdItem = "CorrelationId";
    
    internal const string UnknownPlaceholder =  "unknown";
    
    internal const string CircuitBreakerPolicyName = "CircuitBreakerPolicy";
    internal const string RetryPolicyName = "RetryPolicy";
}