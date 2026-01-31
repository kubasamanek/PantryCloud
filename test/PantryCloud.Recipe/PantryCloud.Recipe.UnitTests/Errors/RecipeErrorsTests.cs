using PantryCloud.Recipe.Core.Errors;
using Shouldly;

namespace PantryCloud.Recipe.UnitTests.Errors;

public class RecipeErrorsTests
{
    [Fact]
    public void RecipeNotFound_ShouldHaveExpectedCodeAndType()
    {
        RecipeErrors.RecipeNotFound.Code.ShouldBe("Recipe.NotFound");
        RecipeErrors.RecipeNotFound.Description.ShouldBe("Recipe not found.");
        RecipeErrors.RecipeNotFound.Type.ShouldBe(ErrorOr.ErrorType.NotFound);
    }

    [Fact]
    public void InvalidSearchRequest_ShouldHaveExpectedCodeAndType()
    {
        RecipeErrors.InvalidSearchRequest.Code.ShouldBe("Recipe.InvalidSearchRequest");
        RecipeErrors.InvalidSearchRequest.Description.ShouldContain("ingredient");
        RecipeErrors.InvalidSearchRequest.Type.ShouldBe(ErrorOr.ErrorType.Validation);
    }
}
