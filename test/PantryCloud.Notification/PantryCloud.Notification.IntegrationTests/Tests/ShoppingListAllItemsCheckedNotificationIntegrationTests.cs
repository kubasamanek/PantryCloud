using System.Collections.Concurrent;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.IntegrationTests.Infrastructure;
using PantryCloud.ShoppingList.Application.Events;
using Shouldly;

namespace PantryCloud.Notification.IntegrationTests.Tests;

[Collection(nameof(IntegrationTestCollection))]
public class ShoppingListAllItemsCheckedNotificationIntegrationTests(NotificationTestFixture fixture)
{
    [Fact]
    public async Task ShoppingListAllItemsChecked_NotifiesWholeHousehold()
    {
        var householdId = Guid.NewGuid();
        var (token, userId) = fixture.CreateTestUser();
        await fixture.SeedHouseholdMembershipAsync(userId, householdId);

        var received = new ConcurrentBag<NotificationDto>();
        var conn = await fixture.CreateSignalRConnectionAsync(token);
        SignalRTestHelper.OnReceiveNotification(conn, received);

        try
        {
            await fixture.PublishEventAsync(new ShoppingListAllItemsCheckedEvent
            {
                HouseholdId = householdId,
                ShoppingListId = Guid.NewGuid(),
                ShoppingListName = "Weekly Groceries",
                CheckedByUserId = userId,
                CorrelationId = Guid.NewGuid().ToString()
            });
            await Task.Delay(2000);

            received.ShouldContain(n => n.Title == "Shopping List Complete" && n.Message.Contains("Weekly Groceries"));
        }
        finally
        {
            await conn.StopAsync();
        }
    }
}
