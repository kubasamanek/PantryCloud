using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Core.Entities;
using PantryCloud.Household.Core.Enums;
using PantryCloud.Household.Core.Errors;
using PantryCloud.Household.Infrastructure.Services;
using Shouldly;

using HouseholdEntity = PantryCloud.Household.Core.Entities.Household;

namespace PantryCloud.Household.UnitTests.Invitation;

public class InvitationServiceTests
{
   private readonly ILogger<InvitationService> _logger = TestHelper.MockLogger<InvitationService>();
   
    [Fact]
    public async Task SendHouseholdInvitation_ShouldCreateInvitation_WhenValidRequest()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(SendHouseholdInvitation_ShouldCreateInvitation_WhenValidRequest));
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);

        db.Members.Add(new HouseholdMember
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            Role = HouseholdRole.Owner
        });
        await db.SaveChangesAsync();

        var service = new InvitationService(_logger, userContext, db);

        var request = new SendHouseholdInvitationRequestDto(Constants.InviteeEmail, Constants.HouseholdId.ToString());

        var result = await service.SendHouseholdInvitation(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Code.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SendHouseholdInvitation_ShouldReturnExistingInvitation_WhenAlreadySent()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(SendHouseholdInvitation_ShouldReturnExistingInvitation_WhenAlreadySent));
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.UserEmail);

        db.Members.Add(new HouseholdMember
        {
            UserId = Constants.UserId,
            HouseholdId = Constants.HouseholdId,
            Role = HouseholdRole.Owner
        });

        db.Invitations.Add(new HouseholdInvitation
        {
            Code = Constants.ValidCode,
            Email = Constants.InviteeEmail,
            HouseholdId = Constants.HouseholdId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });
        await db.SaveChangesAsync();

        var service = new InvitationService(_logger, userContext, db);

        var request = new SendHouseholdInvitationRequestDto(Constants.InviteeEmail, Constants.HouseholdId.ToString());
        
        var result = await service.SendHouseholdInvitation(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Code.ShouldBe(Constants.ValidCode);
    }
    
    [Fact]
    public async Task AcceptHouseholdInvitation_ShouldAddMember_WhenValidCode()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(AcceptHouseholdInvitation_ShouldAddMember_WhenValidCode));
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.InviteeEmail);

        db.Invitations.Add(new HouseholdInvitation
        {
            Code = Constants.ValidCode,
            Email = Constants.InviteeEmail,
            HouseholdId = Constants.HouseholdId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });

        db.Households.Add(new HouseholdEntity
        {
            Id = Constants.HouseholdId,
            Name = "Household"
        });

        await db.SaveChangesAsync();

        var service = new InvitationService(_logger, userContext, db);

        var result = await service.AcceptHouseholdInvitation(new AcceptHouseholdInvitationRequestDto(Constants.ValidCode), CancellationToken.None);

        result.IsError.ShouldBeFalse();

        var member = await db.Members.FirstOrDefaultAsync(m => m.UserId == Constants.UserId);
        member.ShouldNotBeNull();
        member!.HouseholdId.ShouldBe(Constants.HouseholdId);
    }

    [Fact]
    public async Task AcceptHouseholdInvitation_ShouldUpdateMember_WhenAlreadyInHousehold()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(AcceptHouseholdInvitation_ShouldUpdateMember_WhenAlreadyInHousehold));
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.InviteeEmail);

        db.Members.Add(new HouseholdMember
        {
            UserId = Constants.UserId,
            HouseholdId = Guid.NewGuid(), // original household
            Role = HouseholdRole.Member
        });

        db.Invitations.Add(new HouseholdInvitation
        {
            Code = Constants.ValidCode,
            Email = Constants.InviteeEmail,
            HouseholdId = Constants.HouseholdId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });

        db.Households.Add(new HouseholdEntity { Id = Constants.HouseholdId, Name = "Target" });

        await db.SaveChangesAsync();

        var service = new InvitationService(_logger, userContext, db);

        var result = await service.AcceptHouseholdInvitation(new AcceptHouseholdInvitationRequestDto(Constants.ValidCode), CancellationToken.None);

        result.IsError.ShouldBeFalse();

        var member = await db.Members.FirstAsync(m => m.UserId == Constants.UserId);
        member.HouseholdId.ShouldBe(Constants.HouseholdId);
    }

    [Fact]
    public async Task AcceptHouseholdInvitation_ShouldFail_WhenInvitationUsed()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(AcceptHouseholdInvitation_ShouldFail_WhenInvitationUsed));
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.InviteeEmail);

        db.Invitations.Add(new HouseholdInvitation
        {
            Code = Constants.ValidCode,
            Email = Constants.InviteeEmail,
            HouseholdId = Constants.HouseholdId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            UsedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new InvitationService(_logger, userContext, db);

        var result = await service.AcceptHouseholdInvitation(new AcceptHouseholdInvitationRequestDto(Constants.ValidCode), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(InvitationErrors.UsedInvitation);
    }
    
    [Fact]
    public async Task AcceptHouseholdInvitation_ShouldFail_WhenInvitationIsExpired()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(AcceptHouseholdInvitation_ShouldFail_WhenInvitationIsExpired));
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.InviteeEmail);
        db.Invitations.Add(new HouseholdInvitation
        {
            Code = Constants.ValidCode,
            Email = Constants.InviteeEmail,
            HouseholdId = Constants.HouseholdId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1) // Expired
        });
        await db.SaveChangesAsync();

        var service = new InvitationService(_logger, userContext, db);
        var result = await service.AcceptHouseholdInvitation(new AcceptHouseholdInvitationRequestDto(Constants.ValidCode), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(InvitationErrors.ExpiredInvitation);
    }

    [Fact]
    public async Task AcceptHouseholdInvitation_ShouldFail_WhenUserIsOwner()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(AcceptHouseholdInvitation_ShouldFail_WhenUserIsOwner));
        var userContext = TestHelper.CreateMockUserContext(Constants.UserId, Constants.InviteeEmail);

        db.Members.Add(new HouseholdMember
        {
            UserId = Constants.UserId,
            HouseholdId = Guid.NewGuid(),
            Role = HouseholdRole.Owner,
            JoinedAt = DateTime.UtcNow
        });
        db.Invitations.Add(new HouseholdInvitation
        {
            Code = Constants.ValidCode,
            Email = Constants.InviteeEmail,
            HouseholdId = Constants.HouseholdId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });
        db.Households.Add(new HouseholdEntity { Id = Constants.HouseholdId, Name = "Target" });
        await db.SaveChangesAsync();

        var service = new InvitationService(_logger, userContext, db);

        var result = await service.AcceptHouseholdInvitation(new AcceptHouseholdInvitationRequestDto(Constants.ValidCode), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(InvitationErrors.OwnerCannotAcceptInvitation);
    }
}