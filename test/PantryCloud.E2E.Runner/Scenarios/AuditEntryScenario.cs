using PantryCloud.E2E.Runner.Core;
using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class AuditEntryScenario : IE2EScenario
{
    public string Name => "audit-entry";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var (accessToken, householdId) = await ScenarioHelpers.BootstrapUserWithHouseholdAsync(context, cancellationToken);

            var itemName = $"E2E Audit Item {DateTime.UtcNow:O}";
            var created = await context.Pantry.CreateItemAsync(
                accessToken,
                itemName,
                quantity: 1,
                unit: Unit.Piece,
                category: "E2E",
                cancellationToken);

            var from = DateTime.UtcNow.AddMinutes(-5);
            var to = DateTime.UtcNow.AddMinutes(1);
            const int maxAttempts = 10;
            const int delayMilliseconds = 1000;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var result = await context.Audit.ListHouseholdEntriesAsync(
                    accessToken,
                    householdId,
                    from: from,
                    to: to,
                    entityType: "PantryItem",
                    cancellationToken: cancellationToken);

                if (result.Entries is { Count: > 0 })
                {
                    var createEntry = result.Entries.FirstOrDefault(e =>
                        e.EntityId == created.Id &&
                        e.ActionType.Contains("Create", StringComparison.OrdinalIgnoreCase));

                    if (createEntry is not null)
                    {
                        return new E2EScenarioResult(true);
                    }
                }

                await Task.Delay(delayMilliseconds, cancellationToken);
            }

            return new E2EScenarioResult(false,
                "Expected an audit entry for pantry item create within timeout.");
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}
