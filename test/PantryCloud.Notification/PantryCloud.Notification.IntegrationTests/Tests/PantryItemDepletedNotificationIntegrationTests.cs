using System.Collections.Concurrent;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.IntegrationTests.Infrastructure;
using PantryCloud.Pantry.Application.Events;
using Shouldly;

namespace PantryCloud.Notification.IntegrationTests.Tests;

[Collection(nameof(IntegrationTestCollection))]
public class PantryItemDepletedNotificationIntegrationTests(NotificationTestFixture fixture)
{
    [Fact]
    public async Task PantryItemDepleted_NotifiesWholeHousehold()
    {
        var householdId = Guid.NewGuid();
        var (token, userId) = fixture.CreateTestUser();
        await fixture.SeedHouseholdMembershipAsync(userId, householdId);

        var received = new ConcurrentBag<NotificationDto>();
        var conn = await fixture.CreateSignalRConnectionAsync(token);
        SignalRTestHelper.OnReceiveNotification(conn, received);

        try
        {
            await fixture.PublishEventAsync(new PantryItemDepletedEvent
            {
                HouseholdId = householdId,
                ItemId = Guid.NewGuid(),
                ItemName = Constants.TestData.ItemNameMilk,
                InitiatedByUserId = userId,
                CorrelationId = Guid.NewGuid().ToString()
            });
            await Task.Delay(Constants.Delays.DefaultMs);

            received.ShouldContain(n => n.Title == Constants.NotificationTitles.PantryItemDepleted && n.Message.Contains(Constants.TestData.ItemNameMilk));
        }
        finally
        {
            await conn.StopAsync();
        }
    }
}
