using PantryCloud.E2E.Runner.Core;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class RecipeCreateAndSearchScenario : IE2EScenario
{
    public string Name => "recipe-search";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var email = TestIdBuilder.UserEmail(context.Random);
            var password = context.Settings.Auth.DefaultPassword;

            var (_, verifyToken) = await context.Auth.RegisterAsync(email, password, cancellationToken);
            await context.Auth.VerifyEmailAsync(email, verifyToken, cancellationToken);
            var login = await context.Auth.LoginAsync(email, password, cancellationToken);

            var ingredients = new List<string> { "salt", "flour" };
            var searchResult = await context.Recipe.SearchRecipesAsync(
                login.AccessToken,
                ingredients,
                cancellationToken);

            if (searchResult.Recipes is null || searchResult.TotalCount < 0)
            {
                return new E2EScenarioResult(false, "SearchRecipes returned invalid response.");
            }

            if (searchResult.Recipes.Count > 0)
            {
                var firstRecipeId = searchResult.Recipes[0].Id;
                var getResult = await context.Recipe.GetRecipeAsync(
                    login.AccessToken,
                    firstRecipeId,
                    cancellationToken);

                if (getResult is null)
                {
                    return new E2EScenarioResult(false, "GetRecipe returned null for existing recipe id.");
                }

                if (getResult.Recipe.Id != firstRecipeId)
                {
                    return new E2EScenarioResult(false, "GetRecipe returned recipe with mismatched id.");
                }
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}
