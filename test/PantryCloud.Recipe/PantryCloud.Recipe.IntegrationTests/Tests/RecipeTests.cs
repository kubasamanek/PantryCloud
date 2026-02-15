using System.Net;
using System.Text;
using System.Text.Json;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.IntegrationTests.Constants;
using PantryCloud.Recipe.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Recipe.IntegrationTests.Tests;

public class RecipeTests(RecipeTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task Search_Returns401_WhenNoToken()
    {
        var client = Fixture.CreateClient();
        var body = JsonSerializer.Serialize(new SearchRecipesRequestDto(new List<string>()));
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(TestConstants.Endpoints.Search, content);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Search_Returns200_WithEmptyResults_WhenAuthenticated()
    {
        var client = CreateClientWithToken(Guid.NewGuid());
        var body = JsonSerializer.Serialize(new SearchRecipesRequestDto(new List<string>()));
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(TestConstants.Endpoints.Search, content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await GetFromJsonAsync<SearchRecipesResponseDto>(response);
        result.ShouldNotBeNull();
        result.Recipes.ShouldNotBeNull();
        result.Recipes.Count.ShouldBe(0);
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetRecipe_Returns404_WhenNotFound()
    {
        var client = CreateClientWithToken(Guid.NewGuid());
        var recipeId = Guid.NewGuid();

        var response = await client.GetAsync(TestConstants.Endpoints.GetRecipe(recipeId));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRecipe_Returns401_WhenNoToken()
    {
        var client = Fixture.CreateClient();

        var response = await client.GetAsync(TestConstants.Endpoints.GetRecipe(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Seed_Returns200_InDevelopment()
    {
        var client = Fixture.CreateClient();
        var body = JsonSerializer.Serialize(new SeedRecipesRequestDto(5));
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(TestConstants.Endpoints.Seed, content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await GetFromJsonAsync<SeedRecipesResponseDto>(response);
        result.ShouldNotBeNull();
        result.RecipesCreated.ShouldBe(5);
    }

    [Fact]
    public async Task Seed_ThenGetRecipe_Returns200_WhenRecipeExists()
    {
        var seedClient = Fixture.CreateClient();
        var seedBody = JsonSerializer.Serialize(new SeedRecipesRequestDto(3));
        var seedContent = new StringContent(seedBody, Encoding.UTF8, "application/json");
        var seedResponse = await seedClient.PostAsync(TestConstants.Endpoints.Seed, seedContent);
        seedResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var seedResult = await GetFromJsonAsync<SeedRecipesResponseDto>(seedResponse);
        seedResult.ShouldNotBeNull();
        seedResult.RecipesCreated.ShouldBe(3);

        var searchClient = CreateClientWithToken(Guid.NewGuid());
        var searchBody = JsonSerializer.Serialize(new SearchRecipesRequestDto(new List<string>()));
        var searchContent = new StringContent(searchBody, Encoding.UTF8, "application/json");
        var searchResponse = await searchClient.PostAsync(TestConstants.Endpoints.Search, searchContent);
        searchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var searchResult = await GetFromJsonAsync<SearchRecipesResponseDto>(searchResponse);
        searchResult.ShouldNotBeNull();
        searchResult.Recipes.Count.ShouldBeGreaterThan(0);
        var recipeId = searchResult.Recipes[0].Id;

        var getResponse = await searchClient.GetAsync(TestConstants.Endpoints.GetRecipe(recipeId));

        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var getResult = await GetFromJsonAsync<GetRecipeResponseDto>(getResponse);
        getResult.ShouldNotBeNull();
        getResult.Recipe.Id.ShouldBe(recipeId);
    }
}
