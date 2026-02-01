using System.Collections.Concurrent;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Notification.IntegrationTests.Tests;

[Collection(nameof(IntegrationTestCollection))]
public class MemberLeftNotificationIntegrationTests(NotificationTestFixture fixture)
{
    [Fact]
    public async Task MemberLeft_NotifiesRemainingHouseholdMembers()
    {
        var householdId = Guid.NewGuid();
        var (tokenA, userA) = fixture.CreateTestUser();
        var (tokenB, userB) = fixture.CreateTestUser();
        await fixture.SeedHouseholdMembershipAsync(userA, householdId);
        await fixture.SeedHouseholdMembershipAsync(userB, householdId);

        var receivedA = new ConcurrentBag<NotificationDto>();

        var connA = await fixture.CreateSignalRConnectionAsync(tokenA);
        SignalRTestHelper.OnReceiveNotification(connA, receivedA);

        try
        {
            await fixture.PublishEventAsync(new MemberLeftHouseholdEvent
            {
                HouseholdId = householdId,
                MemberId = userB,
                MemberEmail = "userb@test.com",
                LeftAt = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid().ToString()
            });
            await Task.Delay(2000);

            receivedA.ShouldContain(n => n.Title == "Member Left Household");
        }
        finally
        {
            await connA.StopAsync();
        }
    }
}
