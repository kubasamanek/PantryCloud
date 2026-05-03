using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Notification.IntegrationTests.Tests;

[Collection(nameof(IntegrationTestCollection))]
public class GetMyNotificationsIntegrationTests(NotificationTestFixture fixture)
{
    [Fact]
    public async Task GetMyNotifications_ReturnsPersistedNotifications_AfterMemberJoinedEvent()
    {
        var householdId = Guid.NewGuid();
        var (token, userId) = fixture.CreateTestUser();
        await fixture.SeedHouseholdMembershipAsync(userId, householdId);

        await fixture.PublishEventAsync(new MemberJoinedHouseholdEvent
        {
            HouseholdId = householdId,
            NewMemberId = userId,
            MemberEmail = Constants.TestData.UserAEmail,
            JoinedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        });
        await Task.Delay(Constants.Delays.DefaultMs);

        var list = await fixture.GetMyNotificationsAsync(token, 50, null);

        list.ShouldNotBeEmpty();
        list.ShouldContain(n => n.Title == Constants.NotificationTitles.WelcomeToHousehold);
    }

    [Fact]
    public async Task GetMyNotifications_ReturnsEmpty_WhenUserHasNoNotifications()
    {
        var (token, _) = fixture.CreateTestUser();

        var list = await fixture.GetMyNotificationsAsync(token, 50, null);

        list.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetMyNotifications_ReturnsOnlyCurrentUserNotifications()
    {
        var householdId = Guid.NewGuid();
        var (tokenA, userA) = fixture.CreateTestUser();
        var (tokenB, userB) = fixture.CreateTestUser();
        await fixture.SeedHouseholdMembershipAsync(userA, householdId);
        await fixture.SeedHouseholdMembershipAsync(userB, householdId);

        await fixture.PublishEventAsync(new MemberJoinedHouseholdEvent
        {
            HouseholdId = householdId,
            NewMemberId = userB,
            MemberEmail = Constants.TestData.UserBEmail,
            JoinedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        });
        await Task.Delay(Constants.Delays.DefaultMs);

        var listA = await fixture.GetMyNotificationsAsync(tokenA);
        var listB = await fixture.GetMyNotificationsAsync(tokenB);

        listA.ShouldContain(n => n.Title == Constants.NotificationTitles.MemberJoinedHousehold);
        listB.ShouldContain(n => n.Title == Constants.NotificationTitles.WelcomeToHousehold);
    }
}
