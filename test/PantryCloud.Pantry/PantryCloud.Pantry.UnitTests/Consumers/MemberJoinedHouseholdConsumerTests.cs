using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Household.Application.Events;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Infrastructure.Consumers;
using PantryCloud.Pantry.Infrastructure.Persistence;
using Shouldly;

namespace PantryCloud.Pantry.UnitTests.Consumers;

public class MemberJoinedHouseholdConsumerTests
{
    [Fact]
    public async Task Consume_ShouldCreateMembership_WhenNewMember()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldCreateMembership_WhenNewMember));
        var logger = TestHelper.MockLogger<MemberJoinedHouseholdConsumer>();
        var consumer = new MemberJoinedHouseholdConsumer(db, logger);

        var joinedAt = DateTime.UtcNow;
        var @event = new MemberJoinedHouseholdEvent
        {
            HouseholdId = Constants.Household.Id,
            NewMemberId = Constants.User.Id,
            MemberEmail = Constants.User.Email,
            JoinedAt = joinedAt,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberJoinedHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == Constants.User.Id && m.HouseholdId == Constants.Household.Id);

        membership.ShouldNotBeNull();
        membership.LeftAt.ShouldBeNull();
        membership.JoinedAt.ShouldBe(@event.JoinedAt);
    }

    [Fact]
    public async Task Consume_ShouldUpdateMembership_WhenMemberSwitchesHousehold()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldUpdateMembership_WhenMemberSwitchesHousehold));
        var logger = TestHelper.MockLogger<MemberJoinedHouseholdConsumer>();
        var consumer = new MemberJoinedHouseholdConsumer(db, logger);

        var oldJoinedAt = Constants.Dates.ThirtyDaysAgo;

        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.OldId,
            JoinedAt = oldJoinedAt
        });
        await db.SaveChangesAsync();

        var joinedAt = DateTime.UtcNow;
        var @event = new MemberJoinedHouseholdEvent
        {
            HouseholdId = Constants.Household.Id,
            NewMemberId = Constants.User.Id,
            MemberEmail = Constants.User.Email,
            JoinedAt = joinedAt,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberJoinedHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == Constants.User.Id);

        membership.ShouldNotBeNull();
        membership.HouseholdId.ShouldBe(Constants.Household.Id);
        membership.LeftAt.ShouldBeNull();
        // JoinedAt should be updated to the new join date when switching households
        membership.JoinedAt.ShouldBe(joinedAt);
    }

    [Fact]
    public async Task Consume_ShouldReactivateMembership_WhenMemberRejoinsSameHousehold()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldReactivateMembership_WhenMemberRejoinsSameHousehold));
        var logger = TestHelper.MockLogger<MemberJoinedHouseholdConsumer>();
        var consumer = new MemberJoinedHouseholdConsumer(db, logger);

        var leftAt = Constants.Dates.TenDaysAgo;
        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = Constants.User.Id,
            HouseholdId = Constants.Household.Id,
            JoinedAt = Constants.Dates.ThirtyDaysAgo,
            LeftAt = leftAt
        });
        await db.SaveChangesAsync();

        var joinedAt = DateTime.UtcNow;
        var @event = new MemberJoinedHouseholdEvent
        {
            HouseholdId = Constants.Household.Id,
            NewMemberId = Constants.User.Id,
            MemberEmail = Constants.User.Email,
            JoinedAt = joinedAt,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var context = Substitute.For<ConsumeContext<MemberJoinedHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var membership = await db.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == Constants.User.Id && m.HouseholdId == Constants.Household.Id);

        membership.ShouldNotBeNull();
        membership.LeftAt.ShouldBeNull();
    }
}

