using MongoDB.Driver;
using RecipeEntity = PantryCloud.Recipe.Core.Entities.Recipe;

namespace PantryCloud.Recipe.Infrastructure.Persistence;

public class RecipeDbContext(IMongoDatabase database)
{
    public IMongoCollection<RecipeEntity> Recipes => database.GetCollection<RecipeEntity>("recipes");
}

