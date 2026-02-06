using NSubstitute;
using PantryCloud.Audit.Application;
using PantryCloud.Audit.Application.Dtos;
using PantryCloud.Audit.Application.Queries;
using PantryCloud.Audit.Application.Queries.Handlers;
using PantryCloud.SharedKernel.Identity;
using Shouldly;

namespace PantryCloud.Audit.UnitTests.Queries;

public class ListHouseholdAuditEntriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDelegateToAuditQueryService()
    {
        var userId = Constants.Audit.UserId;
        var householdId = Constants.Audit.HouseholdId;
        var request = new ListAuditEntriesRequestDto(householdId, null, null, null, null);
        var query = new ListHouseholdAuditEntriesQuery(request);

        var auditQueryService = Substitute.For<IAuditQueryService>();
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(userId);

        var expectedResponse = new ListAuditEntriesResponseDto([], 0, 1, 50);
        auditQueryService.ListHouseholdAuditEntriesAsync(request, userId, Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        var handler = new ListHouseholdAuditEntriesQueryHandler(auditQueryService, userContext);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(expectedResponse);
        await auditQueryService.Received(1).ListHouseholdAuditEntriesAsync(
            request,
            userId,
            Arg.Any<CancellationToken>());
    }
}
