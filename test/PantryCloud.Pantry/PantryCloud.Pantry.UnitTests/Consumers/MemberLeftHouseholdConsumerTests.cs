using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PantryCloud.Household.Application.Events;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Infrastructure.Consumers;
using Shouldly;

namespace PantryCloud.Pantry.UnitTests.Consumers;

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
        membership.LeftAt.ShouldNotBeNull();
        membership.LeftAt.ShouldBe(leftAt);
    }

    [Fact]
    public async Task Consume_ShouldNotThrow_WhenNoActiveMembershipExists()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldNotThrow_WhenNoActiveMembershipExists));
        var logger = TestHelper.MockLogger<MemberLeftHouseholdConsumer>();
        var consumer = new MemberLeftHouseholdConsumer(db, logger);

        var @event = new MemberLeftHouseholdEvent
        {
            HouseholdId = Constants.Household.Id,
            MemberId = Constants.User.Id,
            MemberEmail = Constants.User.Email,
            LeftAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberLeftHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.HouseholdId == Constants.Household.Id && m.LeftAt == null);

        membership.ShouldBeNull();
    }

    [Fact]
    public async Task Consume_ShouldNotUpdate_WhenMembershipAlreadyLeft()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldNotUpdate_WhenMembershipAlreadyLeft));
        var logger = TestHelper.MockLogger<MemberLeftHouseholdConsumer>();
        var consumer = new MemberLeftHouseholdConsumer(db, logger);

        var originalLeftAt = Constants.Dates.FiveDaysAgo;
        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = Constants.Dates.ThirtyDaysAgo,
            LeftAt = originalLeftAt
        });
        await db.SaveChangesAsync();

        var @event = new MemberLeftHouseholdEvent
        {
            HouseholdId = Constants.Household.Id,
            MemberId = Constants.User.Id,
            MemberEmail = Constants.User.Email,
            LeftAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberLeftHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == Constants.User.Id && m.HouseholdId == Constants.Household.Id);

        membership.ShouldNotBeNull();
        membership.LeftAt.ShouldBe(originalLeftAt);
    }
}

