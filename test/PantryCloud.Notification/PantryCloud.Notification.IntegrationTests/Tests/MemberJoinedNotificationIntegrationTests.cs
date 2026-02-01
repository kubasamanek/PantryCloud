using System.Collections.Concurrent;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Notification.IntegrationTests.Tests;

[Collection(nameof(IntegrationTestCollection))]
public class MemberJoinedNotificationIntegrationTests(NotificationTestFixture fixture)
{
    [Fact]
    public async Task MemberJoined_SendsWelcomeToNewMember_AndJoinedToOthers()
    {
        var householdId = Guid.NewGuid();
        var (tokenA, userA) = fixture.CreateTestUser();
        var (tokenB, userB) = fixture.CreateTestUser();
        await fixture.SeedHouseholdMembershipAsync(userA, householdId);

        var receivedA = new ConcurrentBag<NotificationDto>();
        var receivedB = new ConcurrentBag<NotificationDto>();

        var connA = await fixture.CreateSignalRConnectionAsync(tokenA);
        var connB = await fixture.CreateSignalRConnectionAsync(tokenB);
        SignalRTestHelper.OnReceiveNotification(connA, receivedA);
        SignalRTestHelper.OnReceiveNotification(connB, receivedB);

        try
        {
            await fixture.PublishEventAsync(new MemberJoinedHouseholdEvent
            {
                HouseholdId = householdId,
                NewMemberId = userB,
                MemberEmail = "userb@test.com",
                JoinedAt = DateTime.UtcNow.AddMinutes(-1),
                CorrelationId = Guid.NewGuid().ToString()
            });
            await Task.Delay(1500);

            await fixture.PublishEventAsync(new MemberJoinedHouseholdEvent
            {
                HouseholdId = householdId,
                NewMemberId = userA,
                MemberEmail = "usera@test.com",
                JoinedAt = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid().ToString()
            });
            await Task.Delay(2000);

            receivedA.ShouldContain(n => n.Title == "Welcome to the Household");
            receivedB.ShouldContain(n => n.Title == "Member Joined Household");
        }
        finally
        {
            await connA.StopAsync();
            await connB.StopAsync();
        }
    }
}
