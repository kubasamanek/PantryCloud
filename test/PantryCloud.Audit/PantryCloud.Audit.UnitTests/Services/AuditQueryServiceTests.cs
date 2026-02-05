using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PantryCloud.Audit.Application.Dtos;
using PantryCloud.Audit.Core.Entities;
using PantryCloud.Audit.Core.Errors;
using PantryCloud.Audit.Infrastructure.Persistence;
using PantryCloud.Audit.Infrastructure.Services;
using Shouldly;

namespace PantryCloud.Audit.UnitTests.Services;

public class AuditQueryServiceTests
{
    [Fact]
    public async Task ListHouseholdAuditEntriesAsync_ShouldReturnUserNotInHousehold_WhenUserNotMember()
    {
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var db = TestHelper.CreateInMemoryContext(nameof(ListHouseholdAuditEntriesAsync_ShouldReturnUserNotInHousehold_WhenUserNotMember));
        var membershipRepo = Substitute.For<IHouseholdMembershipRepository>();
        membershipRepo.IsUserInHouseholdAsync(userId, householdId, Arg.Any<CancellationToken>()).Returns(false);

        var service = new AuditQueryService(db, membershipRepo);
        var request = new ListAuditEntriesRequestDto(householdId, null, null, null, null);

        var result = await service.ListHouseholdAuditEntriesAsync(request, userId);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == AuditErrors.UserNotInHousehold.Code);
    }

    [Fact]
    public async Task ListHouseholdAuditEntriesAsync_ShouldReturnEntries_WhenUserIsMember()
    {
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow.AddHours(-1);

        await using var db = TestHelper.CreateInMemoryContext(nameof(ListHouseholdAuditEntriesAsync_ShouldReturnEntries_WhenUserIsMember));
        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = userId,
            HouseholdId = householdId,
            JoinedAt = DateTime.UtcNow.AddDays(-1)
        });
        db.HouseholdAuditEntries.Add(new HouseholdAuditEntry
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            ActionType = "Created",
            EntityType = "PantryItem",
            EntityId = Guid.NewGuid(),
            UserId = userId,
            Payload = "{}",
            OccurredAt = occurredAt,
            EventId = Guid.NewGuid()
        });
        await db.SaveChangesAsync();

        var membershipRepo = Substitute.For<IHouseholdMembershipRepository>();
        membershipRepo.IsUserInHouseholdAsync(userId, householdId, Arg.Any<CancellationToken>()).Returns(true);

        var service = new AuditQueryService(db, membershipRepo);
        var request = new ListAuditEntriesRequestDto(householdId, null, null, null, null);

        var result = await service.ListHouseholdAuditEntriesAsync(request, userId);

        result.IsError.ShouldBeFalse();
        result.Value.Entries.Count.ShouldBe(1);
        result.Value.TotalCount.ShouldBe(1);
        result.Value.Entries[0].ActionType.ShouldBe("Created");
        result.Value.Entries[0].EntityType.ShouldBe("PantryItem");
    }

    [Fact]
    public async Task ListHouseholdAuditEntriesAsync_ShouldApplyFiltersAndPagination_WhenUserIsMember()
    {
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var baseDate = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc);

        await using var db = TestHelper.CreateInMemoryContext(nameof(ListHouseholdAuditEntriesAsync_ShouldApplyFiltersAndPagination_WhenUserIsMember));
        db.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = userId,
            HouseholdId = householdId,
            JoinedAt = baseDate.AddDays(-10)
        });
        db.HouseholdAuditEntries.AddRange(
            CreateAuditEntry(householdId, "Updated", "PantryItem", baseDate.AddDays(0)),
            CreateAuditEntry(householdId, "Created", "PantryItem", baseDate.AddDays(1)),
            CreateAuditEntry(householdId, "Created", "ShoppingList", baseDate.AddDays(2)),
            CreateAuditEntry(householdId, "Joined", "Member", baseDate.AddDays(3)));
        await db.SaveChangesAsync();

        var membershipRepo = Substitute.For<IHouseholdMembershipRepository>();
        membershipRepo.IsUserInHouseholdAsync(userId, householdId, Arg.Any<CancellationToken>()).Returns(true);

        var service = new AuditQueryService(db, membershipRepo);
        var request = new ListAuditEntriesRequestDto(
            householdId,
            baseDate.AddDays(0.5),
            baseDate.AddDays(2.5),
            "Created",
            null,
            Page: 1,
            PageSize: 10);

        var result = await service.ListHouseholdAuditEntriesAsync(request, userId);

        result.IsError.ShouldBeFalse();
        result.Value.Entries.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.Entries.ShouldAllBe(e => e.ActionType == "Created");
        result.Value.Entries.ShouldContain(e => e.EntityType == "PantryItem");
        result.Value.Entries.ShouldContain(e => e.EntityType == "ShoppingList");
    }

    private static HouseholdAuditEntry CreateAuditEntry(Guid householdId, string actionType, string entityType, DateTime occurredAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Payload = "{}",
            OccurredAt = occurredAt,
            EventId = Guid.NewGuid()
        };
}
