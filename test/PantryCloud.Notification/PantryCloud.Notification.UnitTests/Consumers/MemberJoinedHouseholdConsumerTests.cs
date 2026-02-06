using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Entities;
using PantryCloud.Notification.Infrastructure.Consumers;
using PantryCloud.Notification.UnitTests;
using Shouldly;

namespace PantryCloud.Notification.UnitTests.Consumers;

public class MemberJoinedHouseholdConsumerTests
{
    [Fact]
    public async Task Consume_ShouldSendWelcomeAndJoinedNotifications_WhenEventConsumed()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldSendWelcomeAndJoinedNotifications_WhenEventConsumed));
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<MemberJoinedHouseholdConsumer>();
        var consumer = new MemberJoinedHouseholdConsumer(db, notificationService, logger);

        var householdId = Guid.NewGuid();
        var newMemberId = Guid.NewGuid();
        var @event = new MemberJoinedHouseholdEvent
        {
            HouseholdId = householdId,
            NewMemberId = newMemberId,
            MemberEmail = Constants.User.Email,
            JoinedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberJoinedHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await notificationService.Received(1).SendToUserAsync(
            newMemberId,
            Arg.Is<Core.Dtos.NotificationDto>(n =>
                n.Title == Constants.NotificationTitles.WelcomeToHousehold &&
                n.Type == Core.Enums.NotificationType.Success),
            Arg.Any<CancellationToken>());

        await notificationService.Received(1).SendToHouseholdExceptAsync(
            householdId,
            newMemberId,
            Arg.Is<Core.Dtos.NotificationDto>(n =>
                n.Title == Constants.NotificationTitles.MemberJoinedHousehold &&
                n.Type == Core.Enums.NotificationType.Success &&
                n.Message.Contains(Constants.User.Email)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldPersistMembership_WhenEventConsumed()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldPersistMembership_WhenEventConsumed));
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<MemberJoinedHouseholdConsumer>();
        var consumer = new MemberJoinedHouseholdConsumer(db, notificationService, logger);

        var householdId = Guid.NewGuid();
        var newMemberId = Guid.NewGuid();
        var @event = new MemberJoinedHouseholdEvent
        {
            HouseholdId = householdId,
            NewMemberId = newMemberId,
            MemberEmail = Constants.User.Email,
            JoinedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberJoinedHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == newMemberId && m.HouseholdId == householdId);

        membership.ShouldNotBeNull();
        membership.LeftAt.ShouldBeNull();
    }

    [Fact]
    public async Task Consume_ShouldUpdateExistingMembership_WhenMemberRejoinsDifferentHousehold()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldUpdateExistingMembership_WhenMemberRejoinsDifferentHousehold));
        var oldHouseholdId = Guid.NewGuid();
        var newHouseholdId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = memberId,
            HouseholdId = oldHouseholdId,
            JoinedAt = Constants.Dates.OneDayAgo
        });
        await db.SaveChangesAsync();

        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<MemberJoinedHouseholdConsumer>();
        var consumer = new MemberJoinedHouseholdConsumer(db, notificationService, logger);

        var @event = new MemberJoinedHouseholdEvent
        {
            HouseholdId = newHouseholdId,
            NewMemberId = memberId,
            MemberEmail = Constants.User.Email,
            JoinedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberJoinedHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships.FirstOrDefaultAsync(m => m.UserId == memberId);
        membership.ShouldNotBeNull();
        membership.HouseholdId.ShouldBe(newHouseholdId);
        membership.LeftAt.ShouldBeNull();
    }

    [Fact]
    public async Task Consume_ShouldClearLeftAt_WhenMemberRejoinsSameHousehold()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldClearLeftAt_WhenMemberRejoinsSameHousehold));
        var householdId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var leftAt = DateTime.UtcNow.AddHours(-1);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = memberId,
            HouseholdId = householdId,
            JoinedAt = Constants.Dates.OneDayAgo,
            LeftAt = leftAt
        });
        await db.SaveChangesAsync();

        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<MemberJoinedHouseholdConsumer>();
        var consumer = new MemberJoinedHouseholdConsumer(db, notificationService, logger);

        var @event = new MemberJoinedHouseholdEvent
        {
            HouseholdId = householdId,
            NewMemberId = memberId,
            MemberEmail = Constants.User.Email,
            JoinedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberJoinedHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships.FirstOrDefaultAsync(m => m.UserId == memberId && m.HouseholdId == householdId);
        membership.ShouldNotBeNull();
        membership.LeftAt.ShouldBeNull();
    }
}
