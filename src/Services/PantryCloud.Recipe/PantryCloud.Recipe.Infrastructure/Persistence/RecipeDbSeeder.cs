using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PantryCloud.Recipe.Application.Data;

namespace PantryCloud.Recipe.Infrastructure.Persistence;

public class RecipeDbSeeder(RecipeDbContext dbContext, ILogger<RecipeDbSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var count = await dbContext.Recipes
            .CountDocumentsAsync(FilterDefinition<Core.Entities.Recipe>.Empty, cancellationToken: cancellationToken);

        if (count > 0)
        {
            logger.LogInformation("Recipe database already has {Count} recipes, skipping seed", count);
            return;
        }

        var recipes = DefaultRecipeData.GetDefaultRecipes();
        await dbContext.Recipes.InsertManyAsync(recipes, cancellationToken: cancellationToken);
        logger.LogInformation("Seeded {Count} default recipes into the database", recipes.Count);
    }
}
