using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PantryCloud.Audit.Infrastructure.Consumers;
using PantryCloud.Audit.Core.Entities;
using PantryCloud.Household.Application.Events;
using Shouldly;

namespace PantryCloud.Audit.UnitTests.Consumers;

public class MemberLeftHouseholdAuditConsumerTests
{
    [Fact]
    public async Task Consume_ShouldCreateAuditEntryAndUpdateMembership_WhenMemberLeaves()
    {
        var householdId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var leftAt = DateTime.UtcNow;

        await using var db = TestHelper.CreateInMemoryContext(nameof(Consume_ShouldCreateAuditEntryAndUpdateMembership_WhenMemberLeaves));
        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = memberId,
            HouseholdId = householdId,
            JoinedAt = DateTime.UtcNow.AddDays(-10),
            LeftAt = null
        });
        await db.SaveChangesAsync();

        var logger = TestHelper.MockLogger<MemberLeftHouseholdAuditConsumer>();
        var consumer = new MemberLeftHouseholdAuditConsumer(db, logger);

        var @event = new MemberLeftHouseholdEvent
        {
            Id = eventId,
            HouseholdId = householdId,
            MemberId = memberId,
            MemberEmail = Constants.Audit.MemberEmail,
            LeftAt = leftAt,
            CorrelationId = Constants.Audit.CorrelationId
        };

        var context = Substitute.For<ConsumeContext<MemberLeftHouseholdEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        var entries = await db.HouseholdAuditEntries.ToListAsync();
        entries.Count.ShouldBe(1);
        entries[0].ActionType.ShouldBe(Constants.Audit.ActionLeft);
        entries[0].EntityType.ShouldBe(Constants.Audit.EntityMember);
        entries[0].EntityId.ShouldBe(memberId);

        var membership = await db.UserHouseholdMemberships.FirstAsync(m => m.UserId == memberId && m.HouseholdId == householdId);
        membership.LeftAt.ShouldBe(leftAt);
    }
}
