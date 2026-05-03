using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Core.Options;
using PantryCloud.Pantry.Infrastructure.Services;
using PantryCloud.SharedKernel.Enums;
using PantryCloud.SharedKernel.Messaging;
using Shouldly;

namespace PantryCloud.Pantry.UnitTests.Services;

public class ExpirationCheckServiceTests
{
    private readonly ILogger<ExpirationCheckService> _logger = TestHelper.MockLogger<ExpirationCheckService>();

    [Fact]
    public async Task RunAsync_ShouldWriteEventToOutbox_WhenItemsExpiringInNextThreeDays()
    {
        var householdId = Guid.NewGuid();
        var now = new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc);
        var today = now.Date;
        var tomorrow = today.AddDays(1);
        var dayAfterTomorrow = today.AddDays(2);

        await using var db = TestHelper.CreateInMemoryContext(nameof(RunAsync_ShouldWriteEventToOutbox_WhenItemsExpiringInNextThreeDays));
        db.PantryItems.AddRange(
            CreatePantryItem(householdId, Constants.ExpirationCheck.MilkName, today),
            CreatePantryItem(householdId, Constants.ExpirationCheck.BreadName, tomorrow),
            CreatePantryItem(householdId, Constants.ExpirationCheck.EggsName, dayAfterTomorrow));
        await db.SaveChangesAsync();

        var writtenEvents = new List<PantryItemsExpiringSoonEvent>();
        var outboxWriter = Substitute.For<IOutboxWriter>();
        outboxWriter.WriteAsync(Arg.Any<PantryItemsExpiringSoonEvent>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                writtenEvents.Add(call.Arg<PantryItemsExpiringSoonEvent>());
                return Task.CompletedTask;
            });

        var options = Options.Create(new ExpirationCheckOptions { BatchSize = 50, BatchDelayMs = 0 });
        var timeProvider = new Microsoft.Extensions.Time.Testing.FakeTimeProvider(now);

        var service = new ExpirationCheckService(db, outboxWriter, options, timeProvider, _logger);

        await service.RunAsync(CancellationToken.None);

        writtenEvents.ShouldHaveSingleItem();
        var evt = writtenEvents[0];
        evt.HouseholdId.ShouldBe(householdId);
        evt.Items.Count.ShouldBe(3);

        evt.Items.ShouldContain(i => i.Name == Constants.ExpirationCheck.MilkName && i.DaysUntilExpiry == 0);
        evt.Items.ShouldContain(i => i.Name == Constants.ExpirationCheck.BreadName && i.DaysUntilExpiry == 1);
        evt.Items.ShouldContain(i => i.Name == Constants.ExpirationCheck.EggsName && i.DaysUntilExpiry == 2);
    }

    [Fact]
    public async Task RunAsync_ShouldNotWriteToOutbox_WhenNoExpiringItems()
    {
        var householdId = Guid.NewGuid();
        var now = new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc);

        await using var db = TestHelper.CreateInMemoryContext(nameof(RunAsync_ShouldNotWriteToOutbox_WhenNoExpiringItems));
        db.PantryItems.Add(CreatePantryItem(householdId, Constants.ExpirationCheck.MilkName, now.AddDays(5)));
        await db.SaveChangesAsync();

        var outboxWriter = Substitute.For<IOutboxWriter>();
        var options = Options.Create(new ExpirationCheckOptions());
        var timeProvider = new Microsoft.Extensions.Time.Testing.FakeTimeProvider(now);

        var service = new ExpirationCheckService(db, outboxWriter, options, timeProvider, _logger);

        await service.RunAsync(CancellationToken.None);

        await outboxWriter.DidNotReceive().WriteAsync(Arg.Any<PantryItemsExpiringSoonEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_ShouldWriteOneEventPerHousehold_WhenMultipleHouseholdsHaveExpiringItems()
    {
        var household1 = Guid.NewGuid();
        var household2 = Guid.NewGuid();
        var now = new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc);
        var today = now.Date;

        await using var db = TestHelper.CreateInMemoryContext(nameof(RunAsync_ShouldWriteOneEventPerHousehold_WhenMultipleHouseholdsHaveExpiringItems));
        db.PantryItems.Add(CreatePantryItem(household1, Constants.ExpirationCheck.MilkName, today));
        db.PantryItems.Add(CreatePantryItem(household2, Constants.ExpirationCheck.BreadName, today));
        await db.SaveChangesAsync();

        var writtenEvents = new List<PantryItemsExpiringSoonEvent>();
        var outboxWriter = Substitute.For<IOutboxWriter>();
        outboxWriter.WriteAsync(Arg.Any<PantryItemsExpiringSoonEvent>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                writtenEvents.Add(call.Arg<PantryItemsExpiringSoonEvent>());
                return Task.CompletedTask;
            });

        var options = Options.Create(new ExpirationCheckOptions { BatchSize = 50, BatchDelayMs = 0 });
        var timeProvider = new Microsoft.Extensions.Time.Testing.FakeTimeProvider(now);

        var service = new ExpirationCheckService(db, outboxWriter, options, timeProvider, _logger);

        await service.RunAsync(CancellationToken.None);

        writtenEvents.Count.ShouldBe(2);
        writtenEvents.ShouldContain(e => e.HouseholdId == household1 && e.Items.Count == 1 && e.Items[0].Name == Constants.ExpirationCheck.MilkName);
        writtenEvents.ShouldContain(e => e.HouseholdId == household2 && e.Items.Count == 1 && e.Items[0].Name == Constants.ExpirationCheck.BreadName);
    }

    [Fact]
    public async Task RunAsync_ShouldIgnoreItemsWithoutExpirationDate()
    {
        var householdId = Guid.NewGuid();
        var now = new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc);
        var today = now.Date;

        await using var db = TestHelper.CreateInMemoryContext(nameof(RunAsync_ShouldIgnoreItemsWithoutExpirationDate));
        db.PantryItems.Add(CreatePantryItem(householdId, Constants.ExpirationCheck.MilkName, today));
        db.PantryItems.Add(CreatePantryItem(householdId, Constants.ExpirationCheck.NoExpiryName, null));
        await db.SaveChangesAsync();

        var writtenEvents = new List<PantryItemsExpiringSoonEvent>();
        var outboxWriter = Substitute.For<IOutboxWriter>();
        outboxWriter.WriteAsync(Arg.Any<PantryItemsExpiringSoonEvent>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                writtenEvents.Add(call.Arg<PantryItemsExpiringSoonEvent>());
                return Task.CompletedTask;
            });

        var options = Options.Create(new ExpirationCheckOptions());
        var timeProvider = new Microsoft.Extensions.Time.Testing.FakeTimeProvider(now);

        var service = new ExpirationCheckService(db, outboxWriter, options, timeProvider, _logger);

        await service.RunAsync(CancellationToken.None);

        writtenEvents.ShouldHaveSingleItem();
        writtenEvents[0].Items.Count.ShouldBe(1);
        writtenEvents[0].Items[0].Name.ShouldBe(Constants.ExpirationCheck.MilkName);
    }

    private static PantryItem CreatePantryItem(Guid householdId, string name, DateTime? expirationDate)
    {
        return new PantryItem
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            Name = name,
            Quantity = 1,
            Unit = Unit.Piece,
            ExpirationDate = expirationDate,
            CreatedBy = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            RowVersion = Guid.NewGuid().ToByteArray()
        };
    }
}
