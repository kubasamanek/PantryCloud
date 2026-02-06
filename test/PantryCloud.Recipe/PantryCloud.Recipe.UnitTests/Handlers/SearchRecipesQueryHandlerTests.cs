using NSubstitute;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Application.Queries;
using PantryCloud.Recipe.Application.Queries.Handlers;
using Shouldly;

namespace PantryCloud.Recipe.UnitTests.Handlers;

public class SearchRecipesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSearchResults_WhenSearchServiceSucceeds()
    {
        var searchResponse = new SearchRecipesResponseDto(Recipes: [], TotalCount: 0);
        var searchService = Substitute.For<IRecipeSearchService>();
        searchService.SearchRecipesAsync(Arg.Any<SearchRecipesRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(searchResponse);

        var handler = new SearchRecipesQueryHandler(searchService);
        var request = new SearchRecipesRequestDto([..Constants.Search.IngredientsChickenRice]);
        var query = new SearchRecipesQuery(request);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.TotalCount.ShouldBe(0);
        result.Value.Recipes.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldDelegateToSearchService_WithCorrectRequest()
    {
        var searchService = Substitute.For<IRecipeSearchService>();
        var searchResponse = new SearchRecipesResponseDto(Recipes: [], TotalCount: 2);
        searchService.SearchRecipesAsync(Arg.Any<SearchRecipesRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(searchResponse);

        var handler = new SearchRecipesQueryHandler(searchService);
        var request = new SearchRecipesRequestDto([..Constants.Search.IngredientsGarlic]);
        var query = new SearchRecipesQuery(request);

        await handler.Handle(query, CancellationToken.None);

        var expectedIngredients = new List<string>(Constants.Search.IngredientsGarlic);
        await searchService.Received(1).SearchRecipesAsync(
            Arg.Is<SearchRecipesRequestDto>(r => r.Ingredients.SequenceEqual(expectedIngredients)),
            Arg.Any<CancellationToken>());
    }
}
