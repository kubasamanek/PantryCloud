using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Presentation.Validators;
using Shouldly;

namespace PantryCloud.Recipe.UnitTests.Validators;

public class RecommendRecipesRequestValidatorTests
{
    private readonly RecommendRecipesRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_When_LimitIsZero()
    {
        var request = new RecommendRecipesRequestDto(Limit: 0);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RecommendRecipesRequestDto.Limit));
    }

    [Fact]
    public void Should_HaveError_When_LimitExceeds100()
    {
        var request = new RecommendRecipesRequestDto(Limit: 101);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Should_NotHaveError_When_LimitInRange()
    {
        var request = new RecommendRecipesRequestDto(Limit: 50);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Should_NotHaveError_When_ValidRequest()
    {
        var request = new RecommendRecipesRequestDto(
            IngredientHints: ["chicken"],
            Preferences: new PreferencesFilterDto("Vegetarian", ["nuts"]),
            Limit: 10);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Should_HaveError_When_DietaryProfileInvalid()
    {
        var request = new RecommendRecipesRequestDto(
            Preferences: new PreferencesFilterDto("Invalid", null),
            Limit: 10);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }
}
