using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Yarp.ReverseProxy.Forwarder;

namespace PantryCloud.ApiGateway.Infrastructure;

public class ResilientForwarderHttpClientFactory(
    IReadOnlyPolicyRegistry<string> policyRegistry,
    ILogger<ResilientForwarderHttpClientFactory> logger)
    : ForwarderHttpClientFactory
{
    protected override HttpMessageHandler WrapHandler(ForwarderHttpClientContext context, HttpMessageHandler handler)
    {
        var newHandler = base.WrapHandler(context, handler);

        // Circuit Breaker wraps inner handler, Retry wraps Circuit Breaker
        // This ensures retries are attempted only when circuit is closed
        
        // Apply Circuit Breaker Policy if metadata exists (inner layer)
        if (context.NewMetadata?.TryGetValue(Constants.CircuitBreakerPolicyName, out var circuitBreakerPolicyName) == true &&
            policyRegistry.TryGet<IAsyncPolicy<HttpResponseMessage>>(circuitBreakerPolicyName, out var circuitBreakerPolicy))
        {
            logger.LogInformation("Applying Circuit Breaker Policy '{PolicyName}' to Cluster '{ClusterId}'", circuitBreakerPolicyName, context.ClusterId);
            newHandler = new PolicyHttpMessageHandler(circuitBreakerPolicy)
            {
                InnerHandler = newHandler
            };
        }

        // Apply Retry Policy if metadata exists (outer layer - wraps circuit breaker)
        if (context.NewMetadata?.TryGetValue(Constants.RetryPolicyName, out var retryPolicyName) == true &&
            policyRegistry.TryGet<IAsyncPolicy<HttpResponseMessage>>(retryPolicyName, out var retryPolicy))
        {
            logger.LogInformation("Applying Retry Policy '{PolicyName}' to Cluster '{ClusterId}'", retryPolicyName, context.ClusterId);
            newHandler = new PolicyHttpMessageHandler(retryPolicy)
            {
                InnerHandler = newHandler
            };
        }

        return newHandler;
    }
}
