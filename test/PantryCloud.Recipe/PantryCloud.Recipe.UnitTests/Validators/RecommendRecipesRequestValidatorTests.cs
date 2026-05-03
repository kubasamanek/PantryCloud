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
        var request = new RecommendRecipesRequestDto(Limit: Constants.Recommend.Limit0);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RecommendRecipesRequestDto.Limit));
    }

    [Fact]
    public void Should_HaveError_When_LimitExceeds100()
    {
        var request = new RecommendRecipesRequestDto(Limit: Constants.Recommend.Limit101);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Should_NotHaveError_When_LimitInRange()
    {
        var request = new RecommendRecipesRequestDto(Limit: Constants.Recommend.Limit50);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Should_NotHaveError_When_ValidRequest()
    {
        var request = new RecommendRecipesRequestDto(
            IngredientHints: [.. Constants.Recommend.IngredientHintsChicken],
            Preferences: new PreferencesFilterDto(Constants.Preferences.Vegetarian, [.. Constants.Preferences.Nuts]),
            Limit: Constants.Recommend.Limit10);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Should_HaveError_When_DietaryProfileInvalid()
    {
        var request = new RecommendRecipesRequestDto(
            Preferences: new PreferencesFilterDto(Constants.Preferences.Invalid, null),
            Limit: Constants.Recommend.Limit10);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }
}
