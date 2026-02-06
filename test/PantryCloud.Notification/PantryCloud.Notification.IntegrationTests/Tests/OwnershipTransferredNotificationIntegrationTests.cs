using System.Collections.Concurrent;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Notification.IntegrationTests.Tests;

[Collection(nameof(IntegrationTestCollection))]
public class OwnershipTransferredNotificationIntegrationTests(NotificationTestFixture fixture)
{
    [Fact]
    public async Task OwnershipTransferred_NotifiesAllHouseholdMembers()
    {
        var householdId = Guid.NewGuid();
        var previousOwnerId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var (tokenA, userA) = fixture.CreateTestUser();
        var (tokenB, userB) = fixture.CreateTestUser();
        await fixture.SeedHouseholdMembershipAsync(userA, householdId);
        await fixture.SeedHouseholdMembershipAsync(userB, householdId);

        var receivedA = new ConcurrentBag<NotificationDto>();
        var receivedB = new ConcurrentBag<NotificationDto>();

        var connA = await fixture.CreateSignalRConnectionAsync(tokenA);
        var connB = await fixture.CreateSignalRConnectionAsync(tokenB);
        SignalRTestHelper.OnReceiveNotification(connA, receivedA);
        SignalRTestHelper.OnReceiveNotification(connB, receivedB);

        try
        {
            await fixture.PublishEventAsync(new OwnershipTransferredEvent
            {
                HouseholdId = householdId,
                PreviousOwnerId = previousOwnerId,
                NewOwnerId = newOwnerId,
                NewOwnerEmail = Constants.TestData.NewOwnerEmail,
                TransferredAt = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid().ToString()
            });
            await Task.Delay(Constants.Delays.DefaultMs);

            receivedA.ShouldContain(n => n.Title == Constants.NotificationTitles.OwnershipTransferred);
            receivedB.ShouldContain(n => n.Title == Constants.NotificationTitles.OwnershipTransferred);
        }
        finally
        {
            await connA.StopAsync();
            await connB.StopAsync();
        }
    }
}
