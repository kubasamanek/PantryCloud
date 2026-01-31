using NSubstitute;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Application.Queries;
using PantryCloud.Recipe.Application.Queries.Handlers;
using Shouldly;

namespace PantryCloud.Recipe.UnitTests.Handlers;

public class RecommendRecipesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnRecommendations_WhenSearchServiceSucceeds()
    {
        var recommendResponse = new RecommendRecipesResponseDto(Recipes: []);
        var searchService = Substitute.For<IRecipeSearchService>();
        searchService.RecommendRecipesAsync(Arg.Any<RecommendRecipesRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(recommendResponse);

        var handler = new RecommendRecipesQueryHandler(searchService);
        var request = new RecommendRecipesRequestDto(Limit: 10);
        var query = new RecommendRecipesQuery(request);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Recipes.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldDelegateToSearchService_WithCorrectRequest()
    {
        var searchService = Substitute.For<IRecipeSearchService>();
        searchService.RecommendRecipesAsync(Arg.Any<RecommendRecipesRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new RecommendRecipesResponseDto(Recipes: []));

        var handler = new RecommendRecipesQueryHandler(searchService);
        var request = new RecommendRecipesRequestDto(
            IngredientHints: ["chicken"],
            Preferences: new PreferencesFilterDto("Vegetarian", ["nuts"]),
            Limit: 5);
        var query = new RecommendRecipesQuery(request);

        await handler.Handle(query, CancellationToken.None);

        var expectedHints = new List<string> { "chicken" };
        var expectedExcluded = new List<string> { "nuts" };
        await searchService.Received(1).RecommendRecipesAsync(
            Arg.Is<RecommendRecipesRequestDto>(r =>
                r.Limit == 5 &&
                r.IngredientHints!.SequenceEqual(expectedHints) &&
                r.Preferences!.DietaryProfile == "Vegetarian" &&
                r.Preferences.ExcludedIngredients!.SequenceEqual(expectedExcluded)),
            Arg.Any<CancellationToken>());
    }
}
