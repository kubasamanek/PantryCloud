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
        var value = "test-value";
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
        var value = new { Id = 123 };
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
        var error = Error.Unauthorized("Auth.Invalid", "Invalid credentials");
        var result = ErrorOr<string>.From(new List<Error> { error });

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        var problemResult = actionResult.ShouldBeOfType<ObjectResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        var problemDetails = problemResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Detail.ShouldBe("Invalid credentials");
    }

    [Fact]
    public void FromResult_ShouldReturn404_WhenErrorTypeIsNotFound()
    {
        // Arrange
        var error = Error.NotFound("User.NotFound", "User not found");
        var result = ErrorOr<string>.From(new List<Error> { error });

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        var problemResult = actionResult.ShouldBeOfType<ObjectResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        var problemDetails = problemResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Detail.ShouldBe("User not found");
    }

    [Fact]
    public void FromResult_ShouldReturn409_WhenErrorTypeIsConflict()
    {
        // Arrange
        var error = Error.Conflict("User.Exists", "User already exists");
        var result = ErrorOr<string>.From(new List<Error> { error });

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        var problemResult = actionResult.ShouldBeOfType<ObjectResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
        var problemDetails = problemResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Detail.ShouldBe("User already exists");
    }

    [Fact]
    public void FromResult_ShouldReturn400_WhenErrorTypeIsFailure()
    {
        // Arrange
        var error = Error.Failure("Operation.Failed", "Operation failed");
        var result = ErrorOr<string>.From(new List<Error> { error });

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        var problemResult = actionResult.ShouldBeOfType<ObjectResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        var problemDetails = problemResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Detail.ShouldBe("Operation failed");
    }

    [Fact]
    public void FromResult_ShouldReturnValidationProblem_WhenAllErrorsAreValidation()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.Validation("Email", "Email is required"),
            Error.Validation("Password", "Password is too short")
        };
        var result = ErrorOr<string>.From(errors);

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        // ValidationProblem() returns ObjectResult with ValidationProblemDetails
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        var problemDetails = objectResult.Value.ShouldBeOfType<ValidationProblemDetails>();
        problemDetails.Errors.ShouldContainKey("Email");
        problemDetails.Errors.ShouldContainKey("Password");
        problemDetails.Errors["Email"].ShouldContain("Email is required");
        problemDetails.Errors["Password"].ShouldContain("Password is too short");
    }

    [Fact]
    public void FromResult_ShouldReturnFirstError_WhenMultipleNonValidationErrors()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.NotFound("User.NotFound", "User not found"),
            Error.Conflict("User.Exists", "User already exists")
        };
        var result = ErrorOr<string>.From(errors);

        // Act
        var actionResult = _sut.TestFromResult(result, StatusCodes.Status200OK);

        // Assert
        var problemResult = actionResult.ShouldBeOfType<ObjectResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        var problemDetails = problemResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Detail.ShouldBe("User not found");
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

