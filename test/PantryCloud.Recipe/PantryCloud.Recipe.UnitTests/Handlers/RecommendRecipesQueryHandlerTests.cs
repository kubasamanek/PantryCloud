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
        var request = new RecommendRecipesRequestDto(Limit: Constants.Recommend.Limit10);
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
            IngredientHints: [.. Constants.Recommend.IngredientHintsChicken],
            Preferences: new PreferencesFilterDto(Constants.Preferences.Vegetarian, [.. Constants.Preferences.Nuts]),
            Limit: Constants.Recommend.Limit5);
        var query = new RecommendRecipesQuery(request);

        await handler.Handle(query, CancellationToken.None);

        var expectedHints = new List<string>(Constants.Recommend.IngredientHintsChicken);
        var expectedExcluded = new List<string>(Constants.Preferences.Nuts);
        await searchService.Received(1).RecommendRecipesAsync(
            Arg.Is<RecommendRecipesRequestDto>(r =>
                r.Limit == Constants.Recommend.Limit5 &&
                r.IngredientHints!.SequenceEqual(expectedHints) &&
                r.Preferences!.DietaryProfile == Constants.Preferences.Vegetarian &&
                r.Preferences.ExcludedIngredients!.SequenceEqual(expectedExcluded)),
            Arg.Any<CancellationToken>());
    }
}
