using PantryCloud.E2E.Runner.Core;
using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class PantryCrudScenario : IE2EScenario
{
    public string Name => "pantry-crud";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var (accessToken, _) = await ScenarioHelpers.BootstrapUserWithHouseholdAsync(context, cancellationToken);

            var itemName = $"E2E Item {DateTime.UtcNow:O}";
            var category = "E2E-Category";

            var created = await context.Pantry.CreateItemAsync(
                accessToken,
                itemName,
                quantity: 1,
                unit: Unit.Piece,
                category: category,
                cancellationToken);

            var listAfterCreate = await context.Pantry.GetItemsAsync(
                accessToken,
                cancellationToken);

            var createdItem = listAfterCreate.Items.FirstOrDefault(i => i.Id == created.Id);
            if (createdItem is null)
            {
                return new E2EScenarioResult(false, "Created pantry item not found in list.");
            }

            var updatedName = itemName + " Updated";

            var updated = await context.Pantry.UpdateItemAsync(
                accessToken,
                created.Id,
                updatedName,
                quantity: 2,
                unit: Unit.Piece,
                rowVersion: created.RowVersion,
                category: category,
                cancellationToken);

            var listAfterUpdate = await context.Pantry.GetItemsAsync(
                accessToken,
                cancellationToken);

            var updatedItem = listAfterUpdate.Items.FirstOrDefault(i => i.Id == created.Id);
            if (updatedItem is null)
            {
                return new E2EScenarioResult(false, "Updated pantry item not found in list.");
            }

            if (!string.Equals(updatedItem.Name, updatedName, StringComparison.Ordinal) ||
                updatedItem.Quantity != 2)
            {
                return new E2EScenarioResult(false, "Updated pantry item state does not match expected values.");
            }

            await context.Pantry.DeleteItemAsync(
                accessToken,
                created.Id,
                cancellationToken);

            var listAfterDelete = await context.Pantry.GetItemsAsync(
                accessToken,
                cancellationToken);

            if (listAfterDelete.Items.Any(i => i.Id == created.Id))
            {
                return new E2EScenarioResult(false, "Deleted pantry item still present in list.");
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}

