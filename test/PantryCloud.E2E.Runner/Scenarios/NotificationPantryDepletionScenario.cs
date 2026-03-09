using PantryCloud.E2E.Runner.Core;
using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class NotificationPantryDepletionScenario : IE2EScenario
{
    public string Name => "notification-pantry-depletion";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var (accessToken, _) = await ScenarioHelpers.BootstrapUserWithHouseholdAsync(context, cancellationToken);

            var itemName = $"E2E Deplete Notify {DateTime.UtcNow:O}";
            var created = await context.Pantry.CreateItemAsync(
                accessToken,
                itemName,
                quantity: 1,
                unit: Unit.Piece,
                category: "E2E",
                cancellationToken);

            await context.Pantry.UpdateItemAsync(
                accessToken,
                created.Id,
                itemName,
                quantity: 0,
                unit: Unit.Piece,
                rowVersion: created.RowVersion,
                category: "E2E",
                cancellationToken);

            var since = DateTime.UtcNow.AddMinutes(-2);

            const int maxAttempts = 10;
            const int delayMilliseconds = 1000;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var notifications = await context.Notification.GetMyNotificationsAsync(
                    accessToken,
                    limit: 50,
                    since,
                    cancellationToken);

                var depleted = notifications.FirstOrDefault(n =>
                    n.Title.Contains("Depleted", StringComparison.OrdinalIgnoreCase) &&
                    (n.Message.Contains(itemName, StringComparison.OrdinalIgnoreCase) || n.Message.Length > 0));

                if (depleted is not null)
                {
                    return new E2EScenarioResult(true);
                }

                await Task.Delay(delayMilliseconds, cancellationToken);
            }

            return new E2EScenarioResult(false,
                "Expected a pantry depletion notification within timeout.");
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}
