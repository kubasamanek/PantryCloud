using PantryCloud.E2E.Runner.Core;
using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class PantryDepleteScenario : IE2EScenario
{
    public string Name => "pantry-deplete";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var (accessToken, _) = await ScenarioHelpers.BootstrapUserWithHouseholdAsync(context, cancellationToken);

            var itemName = $"E2E Deplete Item {DateTime.UtcNow:O}";
            var category = "E2E-Category";

            var created = await context.Pantry.CreateItemAsync(
                accessToken,
                itemName,
                quantity: 1,
                unit: Unit.Piece,
                category: category,
                cancellationToken);

            var depleted = await context.Pantry.UpdateItemAsync(
                accessToken,
                created.Id,
                itemName,
                quantity: 0,
                unit: Unit.Piece,
                rowVersion: created.RowVersion,
                category: category,
                cancellationToken);

            var listAfterUpdate = await context.Pantry.GetItemsAsync(
                accessToken,
                cancellationToken);

            var item = listAfterUpdate.Items.FirstOrDefault(i => i.Id == created.Id);
            if (item is null)
            {
                return new E2EScenarioResult(false, "Depleted pantry item not found in list.");
            }

            if (item.Quantity != 0)
            {
                return new E2EScenarioResult(false, "Pantry item quantity is not zero after depletion.");
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}

