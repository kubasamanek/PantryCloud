using ErrorOr;
using NSubstitute;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Application.Queries;
using PantryCloud.Recipe.Application.Queries.Handlers;
using PantryCloud.Recipe.Core.Errors;
using PantryCloud.Recipe.Core.Enums;
using PantryCloud.SharedKernel.Enums;
using Shouldly;

namespace PantryCloud.Recipe.UnitTests.Handlers;

public class GetRecipeQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnRecipe_WhenRepositoryReturnsSuccess()
    {
        var recipeDto = CreateMinimalRecipeDto(Constants.Recipe.Id, Constants.Recipe.TestTitle);
        var repo = Substitute.For<IRecipeRepository>();
        repo.GetByIdAsync(Constants.Recipe.Id, Arg.Any<CancellationToken>())
            .Returns(recipeDto);

        var handler = new GetRecipeQueryHandler(repo);
        var query = new GetRecipeQuery(new GetRecipeRequestDto(Constants.Recipe.Id));

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Recipe.Id.ShouldBe(Constants.Recipe.Id);
        result.Value.Recipe.Title.ShouldBe(Constants.Recipe.TestTitle);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenRepositoryReturnsNotFound()
    {
        var repo = Substitute.For<IRecipeRepository>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOr<RecipeDto>.From([RecipeErrors.RecipeNotFound]));

        var handler = new GetRecipeQueryHandler(repo);
        var query = new GetRecipeQuery(new GetRecipeRequestDto(Constants.Recipe.Id));

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == Constants.Errors.RecipeNotFound);
    }

    private static RecipeDto CreateMinimalRecipeDto(Guid id, string title) =>
        new(
            Id: id,
            Title: title,
            Description: null,
            Ingredients: [],
            Steps: [],
            Tags: [],
            DietaryLabels: [],
            Source: RecipeSource.Internal,
            PrepTimeMinutes: 0,
            CookTimeMinutes: 0,
            Servings: 4,
            ImageUrl: null,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow);
}
