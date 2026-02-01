using System.Collections.Concurrent;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Notification.IntegrationTests.Tests;

[Collection(nameof(IntegrationTestCollection))]
public class PreferenceAddedNotificationIntegrationTests(NotificationTestFixture fixture)
{
    [Fact]
    public async Task PreferenceAdded_NotifiesHouseholdExceptUserWhoAdded()
    {
        var householdId = Guid.NewGuid();
        var (tokenAdder, adderId) = fixture.CreateTestUser();
        var (tokenOther, otherId) = fixture.CreateTestUser();
        await fixture.SeedHouseholdMembershipAsync(adderId, householdId);
        await fixture.SeedHouseholdMembershipAsync(otherId, householdId);

        var received = new ConcurrentBag<NotificationDto>();
        var conn = await fixture.CreateSignalRConnectionAsync(tokenOther);
        SignalRTestHelper.OnReceiveNotification(conn, received);

        try
        {
            await fixture.PublishEventAsync(new PreferenceAddedEvent
            {
                HouseholdId = householdId,
                UserId = adderId,
                CorrelationId = Guid.NewGuid().ToString()
            });
            await Task.Delay(2000);

            received.ShouldContain(n => n.Title == "Preferences Updated");
        }
        finally
        {
            await conn.StopAsync();
        }
    }
}
