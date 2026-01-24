using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.SharedKernel.Persistence;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Persistence;

public class ConcurrencyHelperTests
{
    private readonly ILogger _logger;

    public ConcurrencyHelperTests()
    {
        _logger = Substitute.For<ILogger>();
    }

    [Fact]
    public void HandleConcurrencyException_ShouldReturnConflictError_WithDefaultMessage()
    {
        // Arrange
        var exception = new DbUpdateConcurrencyException("Concurrency conflict");
        var entityName = "User";
        var entityId = Guid.NewGuid();

        // Act
        var result = ConcurrencyHelper.HandleConcurrencyException<string>(
            exception,
            _logger,
            entityName,
            entityId);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Conflict);
        result.FirstError.Code.ShouldBe("User.ConcurrencyConflict");
        result.FirstError.Description.ShouldBe("The User was modified by another user. Please refresh and try again.");
    }

    [Fact]
    public void HandleConcurrencyException_ShouldReturnCustomError_WhenProvided()
    {
        // Arrange
        var exception = new DbUpdateConcurrencyException("Concurrency conflict");
        var entityName = "Product";
        var customError = Error.Conflict("Product.StaleData", "Product data is stale");

        // Act
        var result = ConcurrencyHelper.HandleConcurrencyException<string>(
            exception,
            _logger,
            entityName,
            error: customError);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Conflict);
        result.FirstError.Code.ShouldBe("Product.StaleData");
        result.FirstError.Description.ShouldBe("Product data is stale");
    }

    [Fact]
    public void HandleConcurrencyException_ShouldLogWarning_WithEntityId()
    {
        // Arrange
        var exception = new DbUpdateConcurrencyException("Concurrency conflict");
        var entityName = "Order";
        var entityId = Guid.NewGuid();

        // Act
        ConcurrencyHelper.HandleConcurrencyException<string>(
            exception,
            _logger,
            entityName,
            entityId);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Order")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void HandleConcurrencyException_ShouldLogWarning_WithoutEntityId()
    {
        // Arrange
        var exception = new DbUpdateConcurrencyException("Concurrency conflict");
        var entityName = "Invoice";

        // Act
        ConcurrencyHelper.HandleConcurrencyException<string>(
            exception,
            _logger,
            entityName);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Invoice")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ExecuteWithConcurrencyHandling_ShouldReturnResult_WhenOperationSucceeds()
    {
        // Arrange
        var expectedResult = "success";
        Func<Task<string>> operation = () => Task.FromResult(expectedResult);

        // Act
        var result = await ConcurrencyHelper.ExecuteWithConcurrencyHandling(
            operation,
            _logger,
            "User");

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ExecuteWithConcurrencyHandling_ShouldReturnError_WhenConcurrencyExceptionThrown()
    {
        // Arrange
        Func<Task<string>> operation = () => throw new DbUpdateConcurrencyException("Concurrency conflict");

        // Act
        var result = await ConcurrencyHelper.ExecuteWithConcurrencyHandling(
            operation,
            _logger,
            "Product",
            Guid.NewGuid());

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Conflict);
        result.FirstError.Code.ShouldBe("Product.ConcurrencyConflict");
    }

    [Fact]
    public async Task ExecuteWithConcurrencyHandling_ShouldUseCustomError_WhenProvided()
    {
        // Arrange
        var customError = Error.Conflict("Custom.Error", "Custom message");
        Func<Task<string>> operation = () => throw new DbUpdateConcurrencyException("Concurrency conflict");

        // Act
        var result = await ConcurrencyHelper.ExecuteWithConcurrencyHandling(
            operation,
            _logger,
            "Entity",
            error: customError);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Custom.Error");
        result.FirstError.Description.ShouldBe("Custom message");
    }

    [Fact]
    public async Task ExecuteWithConcurrencyHandling_ShouldLogException_WhenConcurrencyExceptionThrown()
    {
        // Arrange
        var exception = new DbUpdateConcurrencyException("Concurrency conflict");
        Func<Task<string>> operation = () => throw exception;

        // Act
        await ConcurrencyHelper.ExecuteWithConcurrencyHandling(
            operation,
            _logger,
            "Category");

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
}

