using System.Collections.Concurrent;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.IntegrationTests.Infrastructure;
using PantryCloud.Pantry.Application.Events;
using Shouldly;

namespace PantryCloud.Notification.IntegrationTests.Tests;

[Collection(nameof(IntegrationTestCollection))]
public class PantryItemsExpiringSoonNotificationIntegrationTests(NotificationTestFixture fixture)
{
    [Fact]
    public async Task PantryItemsExpiringSoon_NotifiesWholeHousehold()
    {
        var householdId = Guid.NewGuid();
        var (token, userId) = fixture.CreateTestUser();
        await fixture.SeedHouseholdMembershipAsync(userId, householdId);

        var received = new ConcurrentBag<NotificationDto>();
        var conn = await fixture.CreateSignalRConnectionAsync(token);
        SignalRTestHelper.OnReceiveNotification(conn, received);

        try
        {
            await fixture.PublishEventAsync(new PantryItemsExpiringSoonEvent
            {
                HouseholdId = householdId,
                Items =
                [
                    new ExpiringItemDto(Guid.NewGuid(), Constants.TestData.ItemNameMilk, DateTime.UtcNow, 0),
                    new ExpiringItemDto(Guid.NewGuid(), Constants.TestData.ItemNameBread, DateTime.UtcNow.AddDays(1), 1)
                ],
                CorrelationId = Guid.NewGuid().ToString()
            });
            await Task.Delay(Constants.Delays.DefaultMs);

            received.ShouldContain(n =>
                n.Title == Constants.NotificationTitles.PantryItemsExpiringSoon &&
                n.Message.Contains(Constants.TestData.ItemNameMilk) &&
                n.Message.Contains("expires today") &&
                n.Message.Contains(Constants.TestData.ItemNameBread) &&
                n.Message.Contains("one day"));
        }
        finally
        {
            await conn.StopAsync();
        }
    }
}
