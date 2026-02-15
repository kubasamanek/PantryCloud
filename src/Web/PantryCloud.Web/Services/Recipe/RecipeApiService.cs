using System.Net.Http.Json;
using PantryCloud.Web.Constants;

namespace PantryCloud.Web.Services.Recipe;

public class RecipeApiService(IHttpClientFactory httpClientFactory) : IRecipeApi
{
    private const string BasePath = ApiPaths.Recipe;
    private HttpClient Client => httpClientFactory.CreateClient("Gateway");

    public async Task<RecommendRecipesResponse?> RecommendAsync(RecommendRecipesRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/recommend", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<RecommendRecipesResponse>(cancellationToken)
            : null;
    }

    public async Task<SearchRecipesResponse?> SearchAsync(SearchRecipesRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/search", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<SearchRecipesResponse>(cancellationToken)
            : null;
    }

    public async Task<GetRecipeResponse?> GetRecipeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync($"{BasePath}/{id}", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GetRecipeResponse>(cancellationToken)
            : null;
    }
}
