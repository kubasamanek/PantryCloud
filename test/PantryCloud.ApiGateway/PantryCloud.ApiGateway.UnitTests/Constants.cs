namespace PantryCloud.ApiGateway.UnitTests;

internal static class Constants
{
    public static class Correlation
    {
        public const string TestCorrelationId = "test-correlation-id-12345";
        public const string TestCorrelationIdShort = "test-correlation-id";
        public const string HeaderName = "X-Correlation-Id";
        public const string ContextItemKey = "CorrelationId";
    }

    public static class Http
    {
        public const string MethodGet = "GET";
        public const string PathApiTest = "/api/test";
        public const int StatusOk = 200;
    }

    public static class Logging
    {
        public const string IncomingRequest = "Incoming request";
        public const string RequestCompleted = "Request completed";
        public const string RequestFailed = "Request failed";
    }

    public static class ResilientForwarder
    {
        public const string TestClusterId = "test-cluster";
        public const string MetadataKeyCircuitBreaker = "CircuitBreakerPolicy";
        public const string MetadataKeyRetry = "RetryPolicy";
        public const string CircuitBreakerPolicyName = "circuit-breaker-policy-name";
        public const string RetryPolicyName = "retry-policy-name";
    }

    public static class Exceptions
    {
        public const string TestException = "Test exception";
    }

    public static class Health
    {
        public const string StatusHealthy = "Healthy";
        public const string ServiceApiGateway = "ApiGateway";
    }
}
