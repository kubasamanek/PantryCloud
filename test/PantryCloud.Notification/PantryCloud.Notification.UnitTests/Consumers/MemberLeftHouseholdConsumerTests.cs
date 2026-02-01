using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Entities;
using PantryCloud.Notification.Infrastructure.Consumers;
using PantryCloud.Notification.Infrastructure.Persistence;
using PantryCloud.Notification.UnitTests;
using Shouldly;

namespace PantryCloud.Notification.UnitTests.Consumers;

public class MemberLeftHouseholdConsumerTests
{
    [Fact]
    public async Task Consume_ShouldSendToHouseholdExcept_WhenEventConsumed()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldSendToHouseholdExcept_WhenEventConsumed));
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<MemberLeftHouseholdConsumer>();
        var consumer = new MemberLeftHouseholdConsumer(db, notificationService, logger);

        var householdId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var @event = new MemberLeftHouseholdEvent
        {
            HouseholdId = householdId,
            MemberId = memberId,
            MemberEmail = "test@example.com",
            LeftAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberLeftHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await notificationService.Received(1).SendToHouseholdExceptAsync(
            householdId,
            memberId,
            Arg.Is<Core.Dtos.NotificationDto>(n =>
                n.Title == "Member Left Household" &&
                n.Type == Core.Enums.NotificationType.Success &&
                n.Message.Contains("test@example.com")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldUpdateMembershipLeftAt_WhenMembershipExists()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldUpdateMembershipLeftAt_WhenMembershipExists));
        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Guid.NewGuid(),
            HouseholdId = Guid.NewGuid(),
            JoinedAt = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var householdId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = memberId,
            HouseholdId = householdId,
            JoinedAt = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<MemberLeftHouseholdConsumer>();
        var consumer = new MemberLeftHouseholdConsumer(db, notificationService, logger);

        var leftAt = DateTime.UtcNow;
        var @event = new MemberLeftHouseholdEvent
        {
            HouseholdId = householdId,
            MemberId = memberId,
            MemberEmail = "test@example.com",
            LeftAt = leftAt,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberLeftHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == memberId && m.HouseholdId == householdId);

        membership.ShouldNotBeNull();
        membership.LeftAt.ShouldBe(leftAt);
    }

    [Fact]
    public async Task Consume_ShouldNotFail_WhenMembershipNotFound()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldNotFail_WhenMembershipNotFound));
        var notificationService = Substitute.For<INotificationService>();
        var logger = TestHelper.MockLogger<MemberLeftHouseholdConsumer>();
        var consumer = new MemberLeftHouseholdConsumer(db, notificationService, logger);

        var householdId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var @event = new MemberLeftHouseholdEvent
        {
            HouseholdId = householdId,
            MemberId = memberId,
            MemberEmail = "orphan@example.com",
            LeftAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberLeftHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await notificationService.Received(1).SendToHouseholdExceptAsync(
            householdId,
            memberId,
            Arg.Is<Core.Dtos.NotificationDto>(n => n.Title == "Member Left Household"),
            Arg.Any<CancellationToken>());
    }
}
