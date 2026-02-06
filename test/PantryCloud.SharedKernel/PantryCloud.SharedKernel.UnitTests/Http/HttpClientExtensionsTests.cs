using Microsoft.AspNetCore.Http;
using NSubstitute;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Http;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Http;

public class HttpClientExtensionsTests
{
    [Fact]
    public void AddCorrelationIdHeader_ShouldAddHeader_WhenCorrelationIdExistsInHttpContext()
    {
        // Arrange
        var correlationId = Constants.Correlation.TestCorrelationId;
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId;
        httpContextAccessor.HttpContext.Returns(httpContext);
        var request = new HttpRequestMessage();

        // Act
        request.AddCorrelationIdHeader(httpContextAccessor);

        // Assert
        request.Headers.Contains(CorrelationIdConstants.HeaderName).ShouldBeTrue();
        request.Headers.GetValues(CorrelationIdConstants.HeaderName).First().ShouldBe(correlationId);
    }

    [Fact]
    public void AddCorrelationIdHeader_ShouldNotAddHeader_WhenCorrelationIdIsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContextAccessor.HttpContext.Returns(httpContext);
        var request = new HttpRequestMessage();

        // Act
        request.AddCorrelationIdHeader(httpContextAccessor);

        // Assert
        request.Headers.Contains(CorrelationIdConstants.HeaderName).ShouldBeFalse();
    }

    [Fact]
    public void AddCorrelationIdHeader_ShouldNotAddHeader_WhenCorrelationIdIsWhitespace()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationIdConstants.HttpContextItemKey] = Constants.Correlation.WhitespaceCorrelationId;
        httpContextAccessor.HttpContext.Returns(httpContext);
        var request = new HttpRequestMessage();

        // Act
        request.AddCorrelationIdHeader(httpContextAccessor);

        // Assert
        request.Headers.Contains(CorrelationIdConstants.HeaderName).ShouldBeFalse();
    }

    [Fact]
    public void AddCorrelationIdHeader_WithProvider_ShouldAddHeader_WhenCorrelationIdExists()
    {
        // Arrange
        var correlationId = Constants.Correlation.ProviderCorrelationId;
        var provider = Substitute.For<ICorrelationIdProvider>();
        provider.GetCorrelationId().Returns(correlationId);
        var request = new HttpRequestMessage();

        // Act
        request.AddCorrelationIdHeader(provider);

        // Assert
        request.Headers.Contains(CorrelationIdConstants.HeaderName).ShouldBeTrue();
        request.Headers.GetValues(CorrelationIdConstants.HeaderName).First().ShouldBe(correlationId);
    }

    [Fact]
    public void AddCorrelationIdHeader_WithProvider_ShouldNotAddHeader_WhenCorrelationIdIsNull()
    {
        // Arrange
        var provider = Substitute.For<ICorrelationIdProvider>();
        provider.GetCorrelationId().Returns((string?)null);
        var request = new HttpRequestMessage();

        // Act
        request.AddCorrelationIdHeader(provider);

        // Assert
        request.Headers.Contains(CorrelationIdConstants.HeaderName).ShouldBeFalse();
    }

    [Fact]
    public void GetCorrelationIdFromContext_ShouldReturnCorrelationId_FromHttpContextItems()
    {
        // Arrange
        var correlationId = Constants.Correlation.ItemsCorrelationId;
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId;
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetCorrelationIdFromContext();

        // Assert
        result.ShouldBe(correlationId);
    }

    [Fact]
    public void GetCorrelationIdFromContext_ShouldReturnCorrelationId_FromHeader()
    {
        // Arrange
        var correlationId = Constants.Correlation.HeaderCorrelationId;
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[CorrelationIdConstants.HeaderName] = correlationId;
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetCorrelationIdFromContext();

        // Assert
        result.ShouldBe(correlationId);
    }

    [Fact]
    public void GetCorrelationIdFromContext_ShouldPreferHttpContextItems_OverHeader()
    {
        // Arrange
        var itemsCorrelationId = Constants.Correlation.ItemsCorrelationIdShort;
        var headerCorrelationId = Constants.Correlation.HeaderCorrelationIdShort;
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationIdConstants.HttpContextItemKey] = itemsCorrelationId;
        httpContext.Request.Headers[CorrelationIdConstants.HeaderName] = headerCorrelationId;
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetCorrelationIdFromContext();

        // Assert
        result.ShouldBe(itemsCorrelationId);
    }

    [Fact]
    public void GetCorrelationIdFromContext_ShouldReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = httpContextAccessor.GetCorrelationIdFromContext();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetCorrelationIdFromContext_ShouldReturnNull_WhenCorrelationIdNotSet()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetCorrelationIdFromContext();

        // Assert
        result.ShouldBeNull();
    }
}


