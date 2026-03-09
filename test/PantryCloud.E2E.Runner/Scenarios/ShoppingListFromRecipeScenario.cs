using PantryCloud.E2E.Runner.Core;
using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class ShoppingListFromRecipeScenario : IE2EScenario
{
    public string Name => "shopping-list-create-with-items";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var (accessToken, _) = await ScenarioHelpers.BootstrapUserWithHouseholdAsync(context, cancellationToken);

            var listName = $"E2E List {DateTime.UtcNow:O}";
            var created = await context.ShoppingList.CreateListAsync(
                accessToken,
                listName,
                cancellationToken);

            if (created.Id == Guid.Empty)
            {
                return new E2EScenarioResult(false, "CreateShoppingList did not return list id.");
            }

            await context.ShoppingList.AddItemAsync(
                accessToken,
                created.Id,
                "Milk",
                1,
                Unit.Liter,
                cancellationToken);

            await context.ShoppingList.AddItemAsync(
                accessToken,
                created.Id,
                "Bread",
                2,
                Unit.Piece,
                cancellationToken);

            var list = await context.ShoppingList.GetListAsync(
                accessToken,
                created.Id,
                cancellationToken);

            if (list.Items.Count < 2)
            {
                return new E2EScenarioResult(false, "Shopping list should contain at least 2 items.");
            }

            var hasMilk = list.Items.Any(i => string.Equals(i.Name, "Milk", StringComparison.OrdinalIgnoreCase));
            var hasBread = list.Items.Any(i => string.Equals(i.Name, "Bread", StringComparison.OrdinalIgnoreCase));
            if (!hasMilk || !hasBread)
            {
                return new E2EScenarioResult(false, "Shopping list missing expected items (Milk, Bread).");
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}
