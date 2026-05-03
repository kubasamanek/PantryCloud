namespace PantryCloud.Web.Services.Recipe;

public interface IRecipeApi
{
    Task<RecommendRecipesResponse?> RecommendAsync(RecommendRecipesRequest request, CancellationToken cancellationToken = default);
    Task<SearchRecipesResponse?> SearchAsync(SearchRecipesRequest request, CancellationToken cancellationToken = default);
    Task<GetRecipeResponse?> GetRecipeAsync(Guid id, CancellationToken cancellationToken = default);
}
