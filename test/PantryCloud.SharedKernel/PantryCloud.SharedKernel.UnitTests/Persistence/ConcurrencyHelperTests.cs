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
        var exception = new DbUpdateConcurrencyException(Constants.Concurrency.ConcurrencyConflictMessage);
        var entityName = Constants.Concurrency.UserEntityName;
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
        result.FirstError.Code.ShouldBe(Constants.Concurrency.UserConcurrencyConflictCode);
        result.FirstError.Description.ShouldBe(Constants.Concurrency.UserConcurrencyConflictDescription);
    }

    [Fact]
    public void HandleConcurrencyException_ShouldReturnCustomError_WhenProvided()
    {
        // Arrange
        var exception = new DbUpdateConcurrencyException(Constants.Concurrency.ConcurrencyConflictMessage);
        var entityName = Constants.Concurrency.ProductEntityName;
        var customError = Error.Conflict(Constants.Concurrency.ProductStaleDataCode, Constants.Concurrency.ProductStaleDataDescription);

        // Act
        var result = ConcurrencyHelper.HandleConcurrencyException<string>(
            exception,
            _logger,
            entityName,
            error: customError);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Conflict);
        result.FirstError.Code.ShouldBe(Constants.Concurrency.ProductStaleDataCode);
        result.FirstError.Description.ShouldBe(Constants.Concurrency.ProductStaleDataDescription);
    }

    [Fact]
    public void HandleConcurrencyException_ShouldLogWarning_WithEntityId()
    {
        // Arrange
        var exception = new DbUpdateConcurrencyException(Constants.Concurrency.ConcurrencyConflictMessage);
        var entityName = Constants.Concurrency.OrderEntityName;
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
            Arg.Is<object>(o => o.ToString()!.Contains(Constants.Concurrency.OrderEntityName)),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void HandleConcurrencyException_ShouldLogWarning_WithoutEntityId()
    {
        // Arrange
        var exception = new DbUpdateConcurrencyException(Constants.Concurrency.ConcurrencyConflictMessage);
        var entityName = Constants.Concurrency.InvoiceEntityName;

        // Act
        ConcurrencyHelper.HandleConcurrencyException<string>(
            exception,
            _logger,
            entityName);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(Constants.Concurrency.InvoiceEntityName)),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ExecuteWithConcurrencyHandling_ShouldReturnResult_WhenOperationSucceeds()
    {
        // Arrange
        var expectedResult = Constants.Concurrency.SuccessResult;
        Func<Task<string>> operation = () => Task.FromResult(expectedResult);

        // Act
        var result = await ConcurrencyHelper.ExecuteWithConcurrencyHandling(
            operation,
            _logger,
            Constants.Concurrency.UserEntityName);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ExecuteWithConcurrencyHandling_ShouldReturnError_WhenConcurrencyExceptionThrown()
    {
        // Arrange
        Func<Task<string>> operation = () => throw new DbUpdateConcurrencyException(Constants.Concurrency.ConcurrencyConflictMessage);

        // Act
        var result = await ConcurrencyHelper.ExecuteWithConcurrencyHandling(
            operation,
            _logger,
            Constants.Concurrency.ProductEntityName,
            Guid.NewGuid());

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Conflict);
        result.FirstError.Code.ShouldBe(Constants.Concurrency.ProductConcurrencyConflictCode);
    }

    [Fact]
    public async Task ExecuteWithConcurrencyHandling_ShouldUseCustomError_WhenProvided()
    {
        // Arrange
        var customError = Error.Conflict(Constants.Concurrency.CustomErrorCode, Constants.Concurrency.CustomErrorMessage);
        Func<Task<string>> operation = () => throw new DbUpdateConcurrencyException(Constants.Concurrency.ConcurrencyConflictMessage);

        // Act
        var result = await ConcurrencyHelper.ExecuteWithConcurrencyHandling(
            operation,
            _logger,
            Constants.Concurrency.EntityEntityName,
            error: customError);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(Constants.Concurrency.CustomErrorCode);
        result.FirstError.Description.ShouldBe(Constants.Concurrency.CustomErrorMessage);
    }

    [Fact]
    public async Task ExecuteWithConcurrencyHandling_ShouldLogException_WhenConcurrencyExceptionThrown()
    {
        // Arrange
        var exception = new DbUpdateConcurrencyException(Constants.Concurrency.ConcurrencyConflictMessage);
        Func<Task<string>> operation = () => throw exception;

        // Act
        await ConcurrencyHelper.ExecuteWithConcurrencyHandling(
            operation,
            _logger,
            Constants.Concurrency.CategoryEntityName);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
}


