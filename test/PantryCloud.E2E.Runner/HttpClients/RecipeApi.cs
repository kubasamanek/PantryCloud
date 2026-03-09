using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PantryCloud.E2E.Runner.Configuration;
using PantryCloud.Recipe.Application.Dtos;

namespace PantryCloud.E2E.Runner.HttpClients;

public sealed class RecipeApi(HttpClient client, E2ESettings settings)
{
    private string VersionedBasePath() =>
        $"/api/v{settings.Gateway.ApiVersion}/recipe/recipes";

    private void SetAuthHeader(string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<SearchRecipesResponseDto> SearchRecipesAsync(
        string accessToken,
        List<string> ingredients,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        var request = new SearchRecipesRequestDto(ingredients, Preferences: null);

        using var response = await client.PostAsJsonAsync(
            $"{VersionedBasePath()}/search",
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"SearchRecipes failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<SearchRecipesResponseDto>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("SearchRecipes response is null.");
        }

        return result;
    }

    public async Task<GetRecipeResponseDto?> GetRecipeAsync(
        string accessToken,
        Guid recipeId,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        using var response = await client.GetAsync(
            $"{VersionedBasePath()}/{recipeId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"GetRecipe failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<GetRecipeResponseDto>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("GetRecipe response is null.");
        }

        return result;
    }
}
