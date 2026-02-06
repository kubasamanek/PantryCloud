using Microsoft.AspNetCore.Http;
using PantryCloud.SharedKernel.Correlation;
using Shouldly;

namespace PantryCloud.ApiGateway.UnitTests.Middleware;

public class CorrelationIdMiddlewareTests
{

    [Fact]
    public async Task InvokeAsync_ShouldGenerateNewCorrelationId_WhenNotProvided()
    {
        var context = TestHelper.CreateHttpContext();
        var nextCalled = false;
        var next = TestHelper.CreateMockNext(ctx => nextCalled = true);
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        nextCalled.ShouldBeTrue();
        context.Response.Headers[Constants.Correlation.HeaderName].ToString().ShouldNotBeNullOrEmpty();
        context.Items[Constants.Correlation.ContextItemKey].ShouldNotBeNull();
        context.Items[Constants.Correlation.ContextItemKey]!.ToString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseExistingCorrelationId_WhenProvided()
    {
        var context = TestHelper.CreateHttpContext(correlationId: Constants.Correlation.TestCorrelationId);
        var nextCalled = false;
        var next = TestHelper.CreateMockNext(ctx => nextCalled = true);
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        nextCalled.ShouldBeTrue();
        context.Response.Headers[Constants.Correlation.HeaderName].ToString().ShouldBe(Constants.Correlation.TestCorrelationId);
        context.Items[Constants.Correlation.ContextItemKey]!.ToString().ShouldBe(Constants.Correlation.TestCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrelationIdInResponseHeaders()
    {
        var context = TestHelper.CreateHttpContext();
        var next = TestHelper.CreateMockNext();
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        context.Response.Headers.ContainsKey(Constants.Correlation.HeaderName).ShouldBeTrue();
        context.Response.Headers[Constants.Correlation.HeaderName].ToString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrelationIdInRequestHeaders()
    {
        var context = TestHelper.CreateHttpContext();
        var next = TestHelper.CreateMockNext();
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        context.Request.Headers.ContainsKey(Constants.Correlation.HeaderName).ShouldBeTrue();
        context.Request.Headers[Constants.Correlation.HeaderName].ToString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrelationIdInContextItems()
    {
        var context = TestHelper.CreateHttpContext();
        var next = TestHelper.CreateMockNext();
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        context.Items.ContainsKey(Constants.Correlation.ContextItemKey).ShouldBeTrue();
        context.Items[Constants.Correlation.ContextItemKey].ShouldNotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        var context = TestHelper.CreateHttpContext();
        var nextCalled = false;
        var next = TestHelper.CreateMockNext(ctx => nextCalled = true);
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        nextCalled.ShouldBeTrue();
    }
}


