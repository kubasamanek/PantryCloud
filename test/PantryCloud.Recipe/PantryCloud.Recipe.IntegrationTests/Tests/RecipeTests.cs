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
        var body = JsonSerializer.Serialize(new SearchRecipesRequestDto(new List<string> { "chicken" }));
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(TestConstants.Endpoints.Search, content);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Search_Returns200_WhenAuthenticated_WithValidRequest()
    {
        var client = CreateClientWithToken(Guid.NewGuid());
        var body = JsonSerializer.Serialize(new SearchRecipesRequestDto(new List<string> { "nonexistent-ingredient-xyz" }));
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(TestConstants.Endpoints.Search, content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await GetFromJsonAsync<SearchRecipesResponseDto>(response);
        result.ShouldNotBeNull();
        result.Recipes.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Search_Returns400_WhenIngredientsEmpty()
    {
        var client = CreateClientWithToken(Guid.NewGuid());
        var body = JsonSerializer.Serialize(new SearchRecipesRequestDto(new List<string>()));
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(TestConstants.Endpoints.Search, content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
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
}
