using ErrorOr;
using PantryCloud.Recipe.Application.Dtos;

namespace PantryCloud.Recipe.Application.Interfaces;

public interface IRecipeRepository
{
    Task<ErrorOr<RecipeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ErrorOr<SeedRecipesResponseDto>> SeedRecipesAsync(List<Core.Entities.Recipe> recipes, CancellationToken cancellationToken = default);
}

