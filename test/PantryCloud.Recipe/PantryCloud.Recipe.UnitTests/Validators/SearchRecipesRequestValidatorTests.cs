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
        var request = new SearchRecipesRequestDto(["chicken", "rice"]);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Should_HaveError_When_IngredientIsEmptyString()
    {
        var request = new SearchRecipesRequestDto(["chicken", ""]);
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Should_HaveError_When_DietaryProfileInvalid()
    {
        var request = new SearchRecipesRequestDto(
            ["chicken"],
            new PreferencesFilterDto("Pescatarian", null));
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Should_NotHaveError_When_DietaryProfileValid()
    {
        var request = new SearchRecipesRequestDto(
            ["chicken"],
            new PreferencesFilterDto("Vegetarian", null));
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Should_NotHaveError_When_ExcludedIngredientsValid()
    {
        var request = new SearchRecipesRequestDto(
            ["chicken"],
            new PreferencesFilterDto(null, ["nuts", "shellfish"]));
        var result = _validator.Validate(request);
        result.IsValid.ShouldBeTrue();
    }
}
