using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PantryCloud.Household.Application.Events;
using PantryCloud.ShoppingList.Core.Entities;
using PantryCloud.ShoppingList.Infrastructure.Consumers;
using Shouldly;

namespace PantryCloud.ShoppingList.UnitTests.Consumers;

public class MemberLeftHouseholdConsumerTests
{
    [Fact]
    public async Task Consume_ShouldMarkMembershipAsLeft_WhenActiveMembershipExists()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldMarkMembershipAsLeft_WhenActiveMembershipExists));
        var logger = TestHelper.MockLogger<MemberLeftHouseholdConsumer>();
        var consumer = new MemberLeftHouseholdConsumer(db, logger);

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = Constants.Dates.ThirtyDaysAgo
        });
        await db.SaveChangesAsync();

        var leftAt = DateTime.UtcNow;
        var @event = new MemberLeftHouseholdEvent
        {
            HouseholdId = Constants.Household.Id,
            MemberId = Constants.User.Id,
            MemberEmail = Constants.User.Email,
            LeftAt = leftAt,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberLeftHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == Constants.User.Id && m.HouseholdId == Constants.Household.Id);

        membership.ShouldNotBeNull();
        membership.LeftAt.ShouldBe(leftAt);
    }

    [Fact]
    public async Task Consume_ShouldDoNothing_WhenNoActiveMembership()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldDoNothing_WhenNoActiveMembership));
        var logger = TestHelper.MockLogger<MemberLeftHouseholdConsumer>();
        var consumer = new MemberLeftHouseholdConsumer(db, logger);

        var leftAt = DateTime.UtcNow;
        var @event = new MemberLeftHouseholdEvent
        {
            HouseholdId = Constants.Household.Id,
            MemberId = Constants.User.Id,
            MemberEmail = Constants.User.Email,
            LeftAt = leftAt,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberLeftHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == Constants.User.Id && m.HouseholdId == Constants.Household.Id);

        membership.ShouldBeNull();
    }

    [Fact]
    public async Task Consume_ShouldDoNothing_WhenMembershipAlreadyLeft()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldDoNothing_WhenMembershipAlreadyLeft));
        var logger = TestHelper.MockLogger<MemberLeftHouseholdConsumer>();
        var consumer = new MemberLeftHouseholdConsumer(db, logger);

        var alreadyLeftAt = Constants.Dates.TenDaysAgo;
        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = Constants.Dates.ThirtyDaysAgo,
            LeftAt = alreadyLeftAt
        });
        await db.SaveChangesAsync();

        var newLeftAt = DateTime.UtcNow;
        var @event = new MemberLeftHouseholdEvent
        {
            HouseholdId = Constants.Household.Id,
            MemberId = Constants.User.Id,
            MemberEmail = Constants.User.Email,
            LeftAt = newLeftAt,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberLeftHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == Constants.User.Id && m.HouseholdId == Constants.Household.Id);

        membership.ShouldNotBeNull();
        membership.LeftAt.ShouldBe(alreadyLeftAt);
    }
}
