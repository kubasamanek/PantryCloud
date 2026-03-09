using PantryCloud.Notification.Core.Dtos;
using PantryCloud.E2E.Runner.Core;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class NotificationMemberJoinedScenario : IE2EScenario
{
    public string Name => "notification-member-joined";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var (ownerToken, memberToken, householdId, _) =
                await ScenarioHelpers.BootstrapOwnerAndMemberInHouseholdAsync(context, cancellationToken);

            var since = DateTime.UtcNow.AddMinutes(-5);

            const int maxAttempts = 10;
            const int delayMilliseconds = 1000;

            IReadOnlyList<NotificationDto> ownerNotifications = Array.Empty<NotificationDto>();

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                ownerNotifications = await context.Notification.GetMyNotificationsAsync(
                    ownerToken,
                    limit: 50,
                    since,
                    cancellationToken);

                var candidate = ownerNotifications.FirstOrDefault(n =>
                    n.Title.Contains("Member Joined", StringComparison.OrdinalIgnoreCase) ||
                    n.Title.Contains("Welcome", StringComparison.OrdinalIgnoreCase));

                if (candidate is not null)
                {
                    return new E2EScenarioResult(true);
                }

                await Task.Delay(delayMilliseconds, cancellationToken);
            }

            var memberJoined = ownerNotifications.FirstOrDefault(n =>
                n.Title.Contains("Member Joined", StringComparison.OrdinalIgnoreCase) ||
                n.Title.Contains("Welcome", StringComparison.OrdinalIgnoreCase));

            if (memberJoined is null)
            {
                return new E2EScenarioResult(false,
                    "Owner should have a notification about member joining. Found: " +
                    string.Join("; ", ownerNotifications.Select(x => x.Title)));
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}
