using PantryCloud.Recipe.Application.Dtos;
using Shouldly;

namespace PantryCloud.Recipe.UnitTests.Validators;

public class SearchRecipesRequestValidatorTests
{
    private readonly TestableSearchRecipesRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_When_IngredientsIsNull()
    {
        var request = new SearchRecipesRequestDto(null!);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(SearchRecipesRequestDto.Ingredients));
    }

    [Fact]
    public void Should_HaveError_When_IngredientsIsEmpty()
    {
        var request = new SearchRecipesRequestDto([]);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(SearchRecipesRequestDto.Ingredients));
    }

    [Fact]
    public void Should_NotHaveError_When_ValidIngredients()
    {
        var request = new SearchRecipesRequestDto([..Constants.Search.IngredientsChickenRice]);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Should_HaveError_When_IngredientIsEmptyString()
    {
        var request = new SearchRecipesRequestDto([..Constants.Search.IngredientsChickenEmpty]);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Should_HaveError_When_DietaryProfileInvalid()
    {
        var request = new SearchRecipesRequestDto(
            [..Constants.Search.IngredientsGarlic],
            new PreferencesFilterDto(Constants.Preferences.Pescatarian, null));
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Should_NotHaveError_When_DietaryProfileValid()
    {
        var request = new SearchRecipesRequestDto(
            [..Constants.Recommend.IngredientHintsChicken],
            new PreferencesFilterDto(Constants.Preferences.Vegetarian, null));
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Should_NotHaveError_When_ExcludedIngredientsValid()
    {
        var request = new SearchRecipesRequestDto(
            [..Constants.Recommend.IngredientHintsChicken],
            new PreferencesFilterDto(null, [..Constants.Preferences.NutsShellfish]));
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeTrue();
    }
}
