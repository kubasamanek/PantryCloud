using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.ApiGateway.Infrastructure;
using Polly;
using Polly.Registry;
using Shouldly;
using Xunit;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Microsoft.Extensions.Http;

namespace PantryCloud.ApiGateway.UnitTests.Infrastructure;

public class ResilientForwarderHttpClientFactoryTests
{
    private readonly IReadOnlyPolicyRegistry<string> _policyRegistry = Substitute.For<IReadOnlyPolicyRegistry<string>>();
    private readonly ILogger<ResilientForwarderHttpClientFactory> _logger = Substitute.For<ILogger<ResilientForwarderHttpClientFactory>>();

    [Fact]
    public void CreateClient_ShouldApplyCircuitBreakerPolicy_WhenMetadataPresent()
    {
        var context = new ForwarderHttpClientContext
        {
            ClusterId = Constants.ResilientForwarder.TestClusterId,
            NewMetadata = new Dictionary<string, string>
            {
                { Constants.ResilientForwarder.MetadataKeyCircuitBreaker, Constants.ResilientForwarder.CircuitBreakerPolicyName }
            },
            NewConfig = new HttpClientConfig()
        };

        var policy = Policy.NoOpAsync<HttpResponseMessage>();
        _policyRegistry.TryGet(Constants.ResilientForwarder.CircuitBreakerPolicyName, out Arg.Any<IAsyncPolicy<HttpResponseMessage>>())
            .Returns(x =>
            {
                x[1] = policy;
                return true;
            });

        var wrapper = new TestableResilientForwarderHttpClientFactory(_policyRegistry, _logger);
        var handler = new HttpClientHandler();

        var resultHandler = wrapper.PublicWrapHandler(context, handler);

        resultHandler.ShouldBeOfType<PolicyHttpMessageHandler>();
    }

    [Fact]
    public void CreateClient_ShouldApplyRetryPolicy_WhenMetadataPresent()
    {
        var context = new ForwarderHttpClientContext
        {
            ClusterId = Constants.ResilientForwarder.TestClusterId,
            NewMetadata = new Dictionary<string, string>
            {
                { Constants.ResilientForwarder.MetadataKeyRetry, Constants.ResilientForwarder.RetryPolicyName }
            },
            NewConfig = new HttpClientConfig()
        };

        var policy = Policy.NoOpAsync<HttpResponseMessage>();
        _policyRegistry.TryGet<IAsyncPolicy<HttpResponseMessage>>(Constants.ResilientForwarder.RetryPolicyName, out Arg.Any<IAsyncPolicy<HttpResponseMessage>>())
            .Returns(x =>
            {
                x[1] = policy;
                return true;
            });

        var wrapper = new TestableResilientForwarderHttpClientFactory(_policyRegistry, _logger);
        var handler = new HttpClientHandler();

        var resultHandler = wrapper.PublicWrapHandler(context, handler);

        resultHandler.ShouldBeOfType<PolicyHttpMessageHandler>();
    }

    [Fact]
    public void CreateClient_ShouldNotApplyPolicy_WhenMetadataMissing()
    {
        var context = new ForwarderHttpClientContext
        {
            ClusterId = Constants.ResilientForwarder.TestClusterId,
            NewMetadata = new Dictionary<string, string>(),
            NewConfig = new HttpClientConfig()
        };

        var wrapper = new TestableResilientForwarderHttpClientFactory(_policyRegistry, _logger);
        var handler = new HttpClientHandler();

        var resultHandler = wrapper.PublicWrapHandler(context, handler);

        resultHandler.ShouldNotBeOfType<PolicyHttpMessageHandler>();
        resultHandler.ShouldBe(handler);
    }

    private class TestableResilientForwarderHttpClientFactory(
        IReadOnlyPolicyRegistry<string> policyRegistry,
        ILogger<ResilientForwarderHttpClientFactory> logger)
        : ResilientForwarderHttpClientFactory(policyRegistry, logger)
    {
        public HttpMessageHandler PublicWrapHandler(ForwarderHttpClientContext context, HttpMessageHandler handler)
        {
            return WrapHandler(context, handler);
        }
    }
}
