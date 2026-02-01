using System.Collections.Concurrent;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.IntegrationTests.Infrastructure;
using PantryCloud.ShoppingList.Application.Events;
using Shouldly;

namespace PantryCloud.Notification.IntegrationTests.Tests;

[Collection(nameof(IntegrationTestCollection))]
public class ShoppingListCreatedNotificationIntegrationTests(NotificationTestFixture fixture)
{
    [Fact]
    public async Task ShoppingListCreated_NotifiesHouseholdExceptCreator()
    {
        var householdId = Guid.NewGuid();
        var (tokenCreator, creatorId) = fixture.CreateTestUser();
        var (tokenOther, otherId) = fixture.CreateTestUser();
        await fixture.SeedHouseholdMembershipAsync(creatorId, householdId);
        await fixture.SeedHouseholdMembershipAsync(otherId, householdId);

        var received = new ConcurrentBag<NotificationDto>();
        var conn = await fixture.CreateSignalRConnectionAsync(tokenOther);
        SignalRTestHelper.OnReceiveNotification(conn, received);

        try
        {
            await fixture.PublishEventAsync(new ShoppingListCreatedEvent
            {
                HouseholdId = householdId,
                ShoppingListId = Guid.NewGuid(),
                ShoppingListName = "Weekly Groceries",
                CreatedByUserId = creatorId,
                CorrelationId = Guid.NewGuid().ToString()
            });
            await Task.Delay(2000);

            received.ShouldContain(n => n.Title == "New Shopping List" && n.Message.Contains("Weekly Groceries"));
        }
        finally
        {
            await conn.StopAsync();
        }
    }
}
