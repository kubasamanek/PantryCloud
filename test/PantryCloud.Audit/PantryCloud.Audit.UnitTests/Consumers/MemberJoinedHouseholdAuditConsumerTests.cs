using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PantryCloud.Audit.Infrastructure.Consumers;
using PantryCloud.Audit.Core.Entities;
using PantryCloud.Household.Application.Events;
using Shouldly;

namespace PantryCloud.Audit.UnitTests.Consumers;

public class MemberJoinedHouseholdAuditConsumerTests
{
    [Fact]
    public async Task Consume_ShouldCreateAuditEntryAndMembership_WhenNewMember()
    {
        var householdId = Guid.NewGuid();
        var newMemberId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var joinedAt = DateTime.UtcNow;

        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldCreateAuditEntryAndMembership_WhenNewMember));
        var logger = TestHelper.MockLogger<MemberJoinedHouseholdAuditConsumer>();
        var consumer = new MemberJoinedHouseholdAuditConsumer(db, logger);

        var @event = new MemberJoinedHouseholdEvent
        {
            Id = eventId,
            HouseholdId = householdId,
            NewMemberId = newMemberId,
            MemberEmail = Constants.Audit.MemberEmail,
            JoinedAt = joinedAt,
            CorrelationId = Constants.Audit.CorrelationId
        };

        var context = Substitute.For<ConsumeContext<MemberJoinedHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var entries = await db.HouseholdAuditEntries.ToListAsync();
        entries.Count.ShouldBe(1);
        entries[0].ActionType.ShouldBe(Constants.Audit.ActionJoined);
        entries[0].EntityType.ShouldBe(Constants.Audit.EntityMember);
        entries[0].EntityId.ShouldBe(newMemberId);

        var memberships = await db.UserHouseholdMemberships.ToListAsync();
        memberships.Count.ShouldBe(1);
        memberships[0].UserId.ShouldBe(newMemberId);
        memberships[0].HouseholdId.ShouldBe(householdId);
    }

    [Fact]
    public async Task Consume_ShouldUpdateExistingMembership_WhenMemberRejoins()
    {
        var householdId = Guid.NewGuid();
        var newMemberId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var joinedAt = DateTime.UtcNow;

        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldUpdateExistingMembership_WhenMemberRejoins));
        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = newMemberId,
            HouseholdId = Guid.NewGuid(),
            JoinedAt = DateTime.UtcNow.AddDays(-10),
            LeftAt = DateTime.UtcNow.AddDays(-5)
        });
        await db.SaveChangesAsync();

        var logger = TestHelper.MockLogger<MemberJoinedHouseholdAuditConsumer>();
        var consumer = new MemberJoinedHouseholdAuditConsumer(db, logger);

        var @event = new MemberJoinedHouseholdEvent
        {
            Id = eventId,
            HouseholdId = householdId,
            NewMemberId = newMemberId,
            JoinedAt = joinedAt,
            CorrelationId = Constants.Audit.CorrelationId
        };

        var context = Substitute.For<ConsumeContext<MemberJoinedHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var memberships = await db.UserHouseholdMemberships.Where(m => m.UserId == newMemberId).ToListAsync();
        memberships.Count.ShouldBe(1);
        memberships[0].HouseholdId.ShouldBe(householdId);
        memberships[0].LeftAt.ShouldBeNull();
    }
}
