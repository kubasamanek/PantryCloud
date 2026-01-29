using ErrorOr;
using PantryCloud.Recipe.Application.Dtos;

namespace PantryCloud.Recipe.Application.Interfaces;

public interface IRecipeSearchService
{
    Task<ErrorOr<SearchRecipesResponseDto>> SearchRecipesAsync(
        SearchRecipesRequestDto request,
        CancellationToken cancellationToken = default);
}


