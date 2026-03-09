using PantryCloud.E2E.Runner.Core;
using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class ShoppingListCompletionScenario : IE2EScenario
{
    public string Name => "shopping-list-completion";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var (accessToken, _) = await ScenarioHelpers.BootstrapUserWithHouseholdAsync(context, cancellationToken);

            var listName = $"E2E Completion List {DateTime.UtcNow:O}";
            var created = await context.ShoppingList.CreateListAsync(
                accessToken,
                listName,
                cancellationToken);

            var item1 = await context.ShoppingList.AddItemAsync(
                accessToken,
                created.Id,
                "Item1",
                1,
                Unit.Piece,
                cancellationToken);

            var item2 = await context.ShoppingList.AddItemAsync(
                accessToken,
                created.Id,
                "Item2",
                1,
                Unit.Piece,
                cancellationToken);

            await context.ShoppingList.CheckItemAsync(
                accessToken,
                created.Id,
                item1.Id,
                cancellationToken);

            var listAfterFirstCheck = await context.ShoppingList.GetListAsync(
                accessToken,
                created.Id,
                cancellationToken);

            var firstItem = listAfterFirstCheck.Items.FirstOrDefault(i => i.Id == item1.Id);
            if (firstItem is null || !firstItem.IsChecked)
            {
                return new E2EScenarioResult(false, "First item should be checked after CheckItem.");
            }

            await context.ShoppingList.CheckItemAsync(
                accessToken,
                created.Id,
                item2.Id,
                cancellationToken);

            var listAfterAllChecked = await context.ShoppingList.GetListAsync(
                accessToken,
                created.Id,
                cancellationToken);

            var allChecked = listAfterAllChecked.Items.All(i => i.IsChecked);
            if (!allChecked)
            {
                return new E2EScenarioResult(false, "All items should be checked after checking the second item.");
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}
