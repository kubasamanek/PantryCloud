using Microsoft.AspNetCore.Http;
using NSubstitute;
using PantryCloud.SharedKernel.Correlation;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Correlation;

public class CorrelationIdProviderTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CorrelationIdProvider _sut;

    public CorrelationIdProviderTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _sut = new CorrelationIdProvider(_httpContextAccessor);
    }

    [Fact]
    public void GetCorrelationId_ShouldReturnCorrelationId_WhenSetInHttpContextItems()
    {
        // Arrange
        var correlationId = "test-correlation-id-123";
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId;
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.GetCorrelationId();

        // Assert
        result.ShouldBe(correlationId);
    }

    [Fact]
    public void GetCorrelationId_ShouldReturnCorrelationId_WhenSetInHeader()
    {
        // Arrange
        var correlationId = "header-correlation-id-456";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[CorrelationIdConstants.HeaderName] = correlationId;
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.GetCorrelationId();

        // Assert
        result.ShouldBe(correlationId);
    }

    [Fact]
    public void GetCorrelationId_ShouldPreferHttpContextItems_OverHeader()
    {
        // Arrange
        var itemsCorrelationId = "items-correlation-id";
        var headerCorrelationId = "header-correlation-id";
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationIdConstants.HttpContextItemKey] = itemsCorrelationId;
        httpContext.Request.Headers[CorrelationIdConstants.HeaderName] = headerCorrelationId;
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.GetCorrelationId();

        // Assert
        result.ShouldBe(itemsCorrelationId);
    }

    [Fact]
    public void GetCorrelationId_ShouldReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = _sut.GetCorrelationId();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetCorrelationId_ShouldReturnNull_WhenCorrelationIdNotSet()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.GetCorrelationId();

        // Assert
        result.ShouldBeNull();
    }
}

