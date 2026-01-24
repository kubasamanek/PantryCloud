using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using PantryCloud.SharedKernel.Behaviors;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidatorsRegistered()
    {
        // Arrange
        var request = new TestRequest { Value = "test" };
        ErrorOr<string> expectedResponse = "success";
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, ErrorOr<string>>(validators);
        var nextCalled = false;
        RequestHandlerDelegate<ErrorOr<string>> next = (CancellationToken ct) =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeTrue();
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe("success");
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationPasses()
    {
        // Arrange
        var request = new TestRequest { Value = "valid" };
        ErrorOr<string> expectedResponse = "success";
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, ErrorOr<string>>(validators);
        var nextCalled = false;
        RequestHandlerDelegate<ErrorOr<string>> next = (CancellationToken ct) =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeTrue();
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe("success");
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationErrors_WhenValidationFails()
    {
        // Arrange
        var request = new TestRequest { Value = "" };
        var validator = Substitute.For<IValidator<TestRequest>>();
        var validationFailures = new List<ValidationFailure>
        {
            new("Value", "Value is required"),
            new("Value", "Value must not be empty")
        };
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, ErrorOr<string>>(validators);
        var nextCalled = false;
        RequestHandlerDelegate<ErrorOr<string>> next = (CancellationToken ct) =>
        {
            nextCalled = true;
            ErrorOr<string> success = "success";
            return Task.FromResult(success);
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeFalse();
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
        result.Errors[0].Type.ShouldBe(ErrorType.Validation);
        result.Errors[0].Code.ShouldBe("Value");
        result.Errors[0].Description.ShouldBe("Value is required");
        result.Errors[1].Description.ShouldBe("Value must not be empty");
    }

    [Fact]
    public async Task Handle_ShouldCombineErrorsFromMultipleValidators()
    {
        // Arrange
        var request = new TestRequest { Value = "" };
        
        var validator1 = Substitute.For<IValidator<TestRequest>>();
        validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Value", "Error from validator 1")
            }));

        var validator2 = Substitute.For<IValidator<TestRequest>>();
        validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("AnotherField", "Error from validator 2")
            }));

        var validators = new[] { validator1, validator2 };
        var behavior = new ValidationBehavior<TestRequest, ErrorOr<string>>(validators);
        RequestHandlerDelegate<ErrorOr<string>> next = (CancellationToken ct) =>
        {
            ErrorOr<string> success = "success";
            return Task.FromResult(success);
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
        result.Errors[0].Code.ShouldBe("Value");
        result.Errors[1].Code.ShouldBe("AnotherField");
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToValidators()
    {
        // Arrange
        var request = new TestRequest { Value = "test" };
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, ErrorOr<string>>(validators);
        var cts = new CancellationTokenSource();
        RequestHandlerDelegate<ErrorOr<string>> next = (CancellationToken ct) =>
        {
            ErrorOr<string> success = "success";
            return Task.FromResult(success);
        };

        // Act
        await behavior.Handle(request, next, cts.Token);

        // Assert
        await validator.Received(1).ValidateAsync(
            Arg.Any<ValidationContext<TestRequest>>(),
            Arg.Is<CancellationToken>(ct => ct == cts.Token));
    }

    public record TestRequest : IRequest<ErrorOr<string>>
    {
        public string Value { get; init; } = string.Empty;
    }
}

