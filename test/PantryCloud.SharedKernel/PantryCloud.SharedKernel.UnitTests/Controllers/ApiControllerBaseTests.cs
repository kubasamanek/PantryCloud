using AutoMapper;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PantryCloud.SharedKernel.Controllers;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Controllers;

public class ApiControllerBaseTests
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly TestController _sut;

    public ApiControllerBaseTests()
    {
        _mediator = Substitute.For<IMediator>();
        _mapper = Substitute.For<IMapper>();
        _sut = new TestController(_mediator, _mapper);
    }

    [Fact]
    public void FromResult_ShouldReturnOkResult_WhenResultIsSuccess()
    {
        // Arrange
        var value = Constants.Controllers.TestValue;
        ErrorOr<string> result = value;

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        objectResult.Value.ShouldBe(value);
    }

    [Fact]
    public void FromResult_ShouldReturnCreatedResult_WhenResultIsSuccessWithCreatedStatusCode()
    {
        // Arrange
        var value = new { Id = Constants.Controllers.TestId };
        ErrorOr<object> result = value;

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status201Created);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        objectResult.Value.ShouldBe(value);
    }

    [Fact]
    public void FromResult_ShouldReturn401_WhenErrorTypeIsUnauthorized()
    {
        // Arrange
        var error = Error.Unauthorized(Constants.Controllers.AuthInvalidCode, Constants.Controllers.InvalidCredentials);
        var result = ErrorOr<string>.From(new List<Error> { error });

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        var problemResult = actionResult.ShouldBeOfType<ObjectResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        var problemDetails = problemResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Detail.ShouldBe(Constants.Controllers.InvalidCredentials);
    }

    [Fact]
    public void FromResult_ShouldReturn404_WhenErrorTypeIsNotFound()
    {
        // Arrange
        var error = Error.NotFound(Constants.Controllers.UserNotFoundCode, Constants.Controllers.UserNotFound);
        var result = ErrorOr<string>.From(new List<Error> { error });

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        var problemResult = actionResult.ShouldBeOfType<ObjectResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        var problemDetails = problemResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Detail.ShouldBe(Constants.Controllers.UserNotFound);
    }

    [Fact]
    public void FromResult_ShouldReturn409_WhenErrorTypeIsConflict()
    {
        // Arrange
        var error = Error.Conflict(Constants.Controllers.UserExistsCode, Constants.Controllers.UserAlreadyExists);
        var result = ErrorOr<string>.From(new List<Error> { error });

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        var problemResult = actionResult.ShouldBeOfType<ObjectResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
        var problemDetails = problemResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Detail.ShouldBe(Constants.Controllers.UserAlreadyExists);
    }

    [Fact]
    public void FromResult_ShouldReturn400_WhenErrorTypeIsFailure()
    {
        // Arrange
        var error = Error.Failure(Constants.Controllers.OperationFailedCode, Constants.Controllers.OperationFailed);
        var result = ErrorOr<string>.From(new List<Error> { error });

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        var problemResult = actionResult.ShouldBeOfType<ObjectResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        var problemDetails = problemResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Detail.ShouldBe(Constants.Controllers.OperationFailed);
    }

    [Fact]
    public void FromResult_ShouldReturnValidationProblem_WhenAllErrorsAreValidation()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.Validation(Constants.Controllers.EmailField, Constants.Controllers.EmailRequired),
            Error.Validation(Constants.Controllers.PasswordField, Constants.Controllers.PasswordTooShort)
        };
        var result = ErrorOr<string>.From(errors);

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        // ValidationProblem() returns ObjectResult with ValidationProblemDetails
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        var problemDetails = objectResult.Value.ShouldBeOfType<ValidationProblemDetails>();
        problemDetails.Errors.ShouldContainKey(Constants.Controllers.EmailField);
        problemDetails.Errors.ShouldContainKey(Constants.Controllers.PasswordField);
        problemDetails.Errors[Constants.Controllers.EmailField].ShouldContain(Constants.Controllers.EmailRequired);
        problemDetails.Errors[Constants.Controllers.PasswordField].ShouldContain(Constants.Controllers.PasswordTooShort);
    }

    [Fact]
    public void FromResult_ShouldReturnFirstError_WhenMultipleNonValidationErrors()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.NotFound(Constants.Controllers.UserNotFoundCode, Constants.Controllers.UserNotFound),
            Error.Conflict(Constants.Controllers.UserExistsCode, Constants.Controllers.UserAlreadyExists)
        };
        var result = ErrorOr<string>.From(errors);

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        var problemResult = actionResult.ShouldBeOfType<ObjectResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        var problemDetails = problemResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Detail.ShouldBe(Constants.Controllers.UserNotFound);
    }

    // Test controller to expose protected methods
    private class TestController(IMediator mediator, IMapper mapper) : ApiControllerBase(mediator, mapper)
    {
        public IActionResult TestFromResult<T>(ErrorOr<T> result, int successStatusCode)
        {
            return FromResult(result, successStatusCode);
        }
    }
}

