using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Core.Entities;
using PantryCloud.Household.Core.Enums;
using PantryCloud.Household.Core.Errors;
using PantryCloud.Household.Infrastructure.Services;
using Shouldly;

namespace PantryCloud.Household.UnitTests.Households;

public class HouseholdManagementServiceTests
{
    private readonly ILogger<HouseholdManagementService> _logger = TestHelper.MockLogger<HouseholdManagementService>();

  [Fact]
    public async Task GetCurrentHousehold_ShouldReturnHousehold_WhenUserIsMember()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetCurrentHousehold_ShouldReturnHousehold_WhenUserIsMember));
        db.Members.Add(new HouseholdMember
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            Role = HouseholdRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        db.Households.Add(new Core.Entities.Household
        {
            Id = Constants.HouseholdId,
            Name = Constants.HouseholdName
        });
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(
            db,
            TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail),
            _logger
        );

        var result = await service.GetCurrentHousehold(CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(Constants.HouseholdId);
        result.Value.Name.ShouldBe(Constants.HouseholdName);
    }

    [Fact]
    public async Task GetCurrentHousehold_ShouldReturnError_WhenUserNotInHousehold()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(GetCurrentHousehold_ShouldReturnError_WhenUserNotInHousehold));

        var service = new HouseholdManagementService(
            db,
            TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail),
            _logger
        );

        var result = await service.GetCurrentHousehold(CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "Household.NotFound");
    }

    [Fact]
    public async Task CreateHousehold_ShouldSucceed_WhenUserNotInAnyHousehold()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(CreateHousehold_ShouldSucceed_WhenUserNotInAnyHousehold));

        var service = new HouseholdManagementService(
            db,
            TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail),
            _logger
        );

        var request = new CreateHouseholdRequestDto("My Household");

        var result = await service.CreateHousehold(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Name.ShouldBe("My Household");

        var member = await db.Members.FirstOrDefaultAsync(m => m.UserId == Constants.UserId);
        member.ShouldNotBeNull();
        member.Role.ShouldBe(HouseholdRole.Owner);
    }

    [Fact]
    public async Task CreateHousehold_ShouldReturnError_WhenUserAlreadyMember()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(CreateHousehold_ShouldReturnError_WhenUserAlreadyMember));
        db.Members.Add(new HouseholdMember
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            Role = HouseholdRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(
            db,
            TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail),
            _logger
        );

        var result = await service.CreateHousehold(new CreateHouseholdRequestDto("Another One"), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.UserAlreadyInHousehold);
    }

    [Fact]
    public async Task LeaveHousehold_ShouldDeleteHousehold_WhenUserIsOnlyMember()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(LeaveHousehold_ShouldDeleteHousehold_WhenUserIsOnlyMember));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Solo House" });
        db.Members.Add(new HouseholdMember
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            Role = HouseholdRole.Owner,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(
            db,
            TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail),
            _logger
        );

        var result = await service.LeaveHousehold(new LeaveHouseholdRequestDto(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        db.Members.Any().ShouldBeFalse();
        db.Households.Any().ShouldBeFalse();
    }

    [Fact]
    public async Task LeaveHousehold_ShouldReturnError_WhenOwnerAndNotAlone()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(LeaveHousehold_ShouldReturnError_WhenOwnerAndNotAlone));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Team House" });
        db.Members.AddRange(
            new HouseholdMember
            {
                UserId = Constants.UserId,
                HouseholdId = Constants.HouseholdId,
                Role = HouseholdRole.Owner,
                JoinedAt = DateTime.UtcNow
            },
            new HouseholdMember
            {
                UserId = Guid.NewGuid(),
                HouseholdId = Constants.HouseholdId,
                Role = HouseholdRole.Member,
                JoinedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(
            db,
            TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail),
            _logger
        );

        var result = await service.LeaveHousehold(new LeaveHouseholdRequestDto(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.OwnerCannotLeave);
    }

    [Fact]
    public async Task LeaveHousehold_ShouldRemoveMember_WhenNonOwner()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(LeaveHousehold_ShouldRemoveMember_WhenNonOwner));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Shared House" });
        db.Members.AddRange(
            new HouseholdMember
            {
                UserId = Constants.UserId,
                HouseholdId = Constants.HouseholdId,
                Role = HouseholdRole.Member,
                JoinedAt = DateTime.UtcNow
            },
            new HouseholdMember
            {
                UserId = Guid.NewGuid(),
                HouseholdId = Constants.HouseholdId,
                Role = HouseholdRole.Owner,
                JoinedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(
            db,
            TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail),
            _logger
        );

        var result = await service.LeaveHousehold(new LeaveHouseholdRequestDto(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        db.Members.Count().ShouldBe(1);
        db.Members.Single().Role.ShouldBe(HouseholdRole.Owner);
    }

    [Fact]
    public async Task LeaveHousehold_ShouldReturnError_WhenUserNotMember()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(LeaveHousehold_ShouldReturnError_WhenUserNotMember));

        var service = new HouseholdManagementService(
            db,
            TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail),
            _logger
        );

        var result = await service.LeaveHousehold(new LeaveHouseholdRequestDto(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.UserNotInAnyHousehold);
    }

    [Fact]
    public async Task KickMember_ShouldSucceed_WhenOwnerKicksMember()
    {
        var memberToKickId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        await using var db = TestHelper.CreateInMemoryContext(nameof(KickMember_ShouldSucceed_WhenOwnerKicksMember));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Team House" });
        db.Members.AddRange(
            new HouseholdMember { UserId = Constants.UserId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Owner, JoinedAt = DateTime.UtcNow },
            new HouseholdMember { UserId = memberToKickId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Member, JoinedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(db, TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail), _logger);

        var result = await service.KickMemberAsync(new KickMemberRequestDto(memberToKickId), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.HouseholdId.ShouldBe(Constants.HouseholdId);
        result.Value.KickedUserId.ShouldBe(memberToKickId);
        db.Members.Count().ShouldBe(1);
        db.Members.Single().UserId.ShouldBe(Constants.UserId);
    }

    [Fact]
    public async Task KickMember_ShouldReturnError_WhenCallerNotOwner()
    {
        var memberToKickId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        await using var db = TestHelper.CreateInMemoryContext(nameof(KickMember_ShouldReturnError_WhenCallerNotOwner));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Team House" });
        db.Members.AddRange(
            new HouseholdMember { UserId = Constants.UserId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Member, JoinedAt = DateTime.UtcNow },
            new HouseholdMember { UserId = memberToKickId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Owner, JoinedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(db, TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail), _logger);

        var result = await service.KickMemberAsync(new KickMemberRequestDto(memberToKickId), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.UserNotOwner);
    }

    [Fact]
    public async Task KickMember_ShouldReturnError_WhenKickingOwner()
    {
        var otherOwnerId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        await using var db = TestHelper.CreateInMemoryContext(nameof(KickMember_ShouldReturnError_WhenKickingOwner));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Team House" });
        db.Members.AddRange(
            new HouseholdMember { UserId = Constants.UserId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Owner, JoinedAt = DateTime.UtcNow },
            new HouseholdMember { UserId = otherOwnerId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Owner, JoinedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(db, TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail), _logger);

        var result = await service.KickMemberAsync(new KickMemberRequestDto(otherOwnerId), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.CannotKickOwner);
    }

    [Fact]
    public async Task KickMember_ShouldReturnError_WhenKickingSelf()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(KickMember_ShouldReturnError_WhenKickingSelf));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Solo House" });
        db.Members.Add(new HouseholdMember { UserId = Constants.UserId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Owner, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(db, TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail), _logger);

        var result = await service.KickMemberAsync(new KickMemberRequestDto(Constants.UserId), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.CannotKickSelf);
    }

    [Fact]
    public async Task KickMember_ShouldReturnError_WhenTargetNotInHousehold()
    {
        var nonMemberId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        await using var db = TestHelper.CreateInMemoryContext(nameof(KickMember_ShouldReturnError_WhenTargetNotInHousehold));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Team House" });
        db.Members.Add(new HouseholdMember { UserId = Constants.UserId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Owner, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(db, TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail), _logger);

        var result = await service.KickMemberAsync(new KickMemberRequestDto(nonMemberId), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.MemberNotFoundInHousehold);
    }

    [Fact]
    public async Task TransferOwnership_ShouldSucceed_WhenValidMember()
    {
        var newOwnerId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        await using var db = TestHelper.CreateInMemoryContext(nameof(TransferOwnership_ShouldSucceed_WhenValidMember));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Team House" });
        db.Members.AddRange(
            new HouseholdMember { UserId = Constants.UserId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Owner, JoinedAt = DateTime.UtcNow },
            new HouseholdMember { UserId = newOwnerId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Member, JoinedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(db, TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail), _logger);

        var result = await service.TransferOwnershipAsync(new TransferOwnershipRequestDto(newOwnerId), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.HouseholdId.ShouldBe(Constants.HouseholdId);
        result.Value.PreviousOwnerId.ShouldBe(Constants.UserId);
        result.Value.NewOwnerId.ShouldBe(newOwnerId);

        var previousOwner = await db.Members.FirstAsync(m => m.UserId == Constants.UserId);
        var newOwner = await db.Members.FirstAsync(m => m.UserId == newOwnerId);
        previousOwner.Role.ShouldBe(HouseholdRole.Member);
        newOwner.Role.ShouldBe(HouseholdRole.Owner);
    }

    [Fact]
    public async Task TransferOwnership_ShouldReturnError_WhenCallerNotOwner()
    {
        var newOwnerId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        await using var db = TestHelper.CreateInMemoryContext(nameof(TransferOwnership_ShouldReturnError_WhenCallerNotOwner));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Team House" });
        db.Members.AddRange(
            new HouseholdMember { UserId = Constants.UserId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Member, JoinedAt = DateTime.UtcNow },
            new HouseholdMember { UserId = newOwnerId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Owner, JoinedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(db, TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail), _logger);

        var result = await service.TransferOwnershipAsync(new TransferOwnershipRequestDto(newOwnerId), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.UserNotOwner);
    }

    [Fact]
    public async Task TransferOwnership_ShouldReturnError_WhenNewOwnerNotMember()
    {
        var nonMemberId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        await using var db = TestHelper.CreateInMemoryContext(nameof(TransferOwnership_ShouldReturnError_WhenNewOwnerNotMember));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Team House" });
        db.Members.Add(new HouseholdMember { UserId = Constants.UserId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Owner, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(db, TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail), _logger);

        var result = await service.TransferOwnershipAsync(new TransferOwnershipRequestDto(nonMemberId), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.NewOwnerMustBeMember);
    }

    [Fact]
    public async Task TransferOwnership_ShouldReturnError_WhenTransferToSelf()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(TransferOwnership_ShouldReturnError_WhenTransferToSelf));
        db.Households.Add(new Core.Entities.Household { Id = Constants.HouseholdId, Name = "Solo House" });
        db.Members.Add(new HouseholdMember { UserId = Constants.UserId, HouseholdId = Constants.HouseholdId, Role = HouseholdRole.Owner, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new HouseholdManagementService(db, TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail), _logger);

        var result = await service.TransferOwnershipAsync(new TransferOwnershipRequestDto(Constants.UserId), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(HouseholdErrors.CannotTransferToSelf);
    }
}