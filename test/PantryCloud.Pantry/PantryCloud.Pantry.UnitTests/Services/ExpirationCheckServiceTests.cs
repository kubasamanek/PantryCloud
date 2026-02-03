using Microsoft.EntityFrameworkCore;
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
    public async Task RunAsync_ShouldPublishEvent_WhenItemsExpiringInNextThreeDays()
    {
        var householdId = Guid.NewGuid();
        var now = new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc);
        var today = now.Date;
        var tomorrow = today.AddDays(1);
        var dayAfterTomorrow = today.AddDays(2);

        await using var db = TestHelper.CreateInMemoryContext(nameof(RunAsync_ShouldPublishEvent_WhenItemsExpiringInNextThreeDays));
        db.PantryItems.AddRange(
            CreatePantryItem(householdId, "Milk", today),
            CreatePantryItem(householdId, "Bread", tomorrow),
            CreatePantryItem(householdId, "Eggs", dayAfterTomorrow));
        await db.SaveChangesAsync();

        var publishedEvents = new List<PantryItemsExpiringSoonEvent>();
        var messageBus = Substitute.For<IMessageBus>();
        messageBus.PublishAsync(Arg.Any<PantryItemsExpiringSoonEvent>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                publishedEvents.Add(call.Arg<PantryItemsExpiringSoonEvent>());
                return Task.CompletedTask;
            });

        var options = Options.Create(new ExpirationCheckOptions { BatchSize = 50, BatchDelayMs = 0 });
        var timeProvider = new Microsoft.Extensions.Time.Testing.FakeTimeProvider(now);

        var service = new ExpirationCheckService(db, messageBus, options, timeProvider, _logger);

        await service.RunAsync(CancellationToken.None);

        publishedEvents.ShouldHaveSingleItem();
        var evt = publishedEvents[0];
        evt.HouseholdId.ShouldBe(householdId);
        evt.Items.Count.ShouldBe(3);

        evt.Items.ShouldContain(i => i.Name == "Milk" && i.DaysUntilExpiry == 0);
        evt.Items.ShouldContain(i => i.Name == "Bread" && i.DaysUntilExpiry == 1);
        evt.Items.ShouldContain(i => i.Name == "Eggs" && i.DaysUntilExpiry == 2);
    }

    [Fact]
    public async Task RunAsync_ShouldNotPublish_WhenNoExpiringItems()
    {
        var householdId = Guid.NewGuid();
        var now = new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc);

        await using var db = TestHelper.CreateInMemoryContext(nameof(RunAsync_ShouldNotPublish_WhenNoExpiringItems));
        db.PantryItems.Add(CreatePantryItem(householdId, "Milk", now.AddDays(5)));
        await db.SaveChangesAsync();

        var messageBus = Substitute.For<IMessageBus>();
        var options = Options.Create(new ExpirationCheckOptions());
        var timeProvider = new Microsoft.Extensions.Time.Testing.FakeTimeProvider(now);

        var service = new ExpirationCheckService(db, messageBus, options, timeProvider, _logger);

        await service.RunAsync(CancellationToken.None);

        await messageBus.DidNotReceive().PublishAsync(Arg.Any<PantryItemsExpiringSoonEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_ShouldPublishPerHousehold_WhenMultipleHouseholdsHaveExpiringItems()
    {
        var household1 = Guid.NewGuid();
        var household2 = Guid.NewGuid();
        var now = new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc);
        var today = now.Date;

        await using var db = TestHelper.CreateInMemoryContext(nameof(RunAsync_ShouldPublishPerHousehold_WhenMultipleHouseholdsHaveExpiringItems));
        db.PantryItems.Add(CreatePantryItem(household1, "Milk", today));
        db.PantryItems.Add(CreatePantryItem(household2, "Bread", today));
        await db.SaveChangesAsync();

        var publishedEvents = new List<PantryItemsExpiringSoonEvent>();
        var messageBus = Substitute.For<IMessageBus>();
        messageBus.PublishAsync(Arg.Any<PantryItemsExpiringSoonEvent>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                publishedEvents.Add(call.Arg<PantryItemsExpiringSoonEvent>());
                return Task.CompletedTask;
            });

        var options = Options.Create(new ExpirationCheckOptions { BatchSize = 50, BatchDelayMs = 0 });
        var timeProvider = new Microsoft.Extensions.Time.Testing.FakeTimeProvider(now);

        var service = new ExpirationCheckService(db, messageBus, options, timeProvider, _logger);

        await service.RunAsync(CancellationToken.None);

        publishedEvents.Count.ShouldBe(2);
        publishedEvents.ShouldContain(e => e.HouseholdId == household1 && e.Items.Count == 1 && e.Items[0].Name == "Milk");
        publishedEvents.ShouldContain(e => e.HouseholdId == household2 && e.Items.Count == 1 && e.Items[0].Name == "Bread");
    }

    [Fact]
    public async Task RunAsync_ShouldIgnoreItemsWithoutExpirationDate()
    {
        var householdId = Guid.NewGuid();
        var now = new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc);
        var today = now.Date;

        await using var db = TestHelper.CreateInMemoryContext(nameof(RunAsync_ShouldIgnoreItemsWithoutExpirationDate));
        db.PantryItems.Add(CreatePantryItem(householdId, "Milk", today));
        db.PantryItems.Add(CreatePantryItem(householdId, "No expiry", null));
        await db.SaveChangesAsync();

        var publishedEvents = new List<PantryItemsExpiringSoonEvent>();
        var messageBus = Substitute.For<IMessageBus>();
        messageBus.PublishAsync(Arg.Any<PantryItemsExpiringSoonEvent>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                publishedEvents.Add(call.Arg<PantryItemsExpiringSoonEvent>());
                return Task.CompletedTask;
            });

        var options = Options.Create(new ExpirationCheckOptions());
        var timeProvider = new Microsoft.Extensions.Time.Testing.FakeTimeProvider(now);

        var service = new ExpirationCheckService(db, messageBus, options, timeProvider, _logger);

        await service.RunAsync(CancellationToken.None);

        publishedEvents.ShouldHaveSingleItem();
        publishedEvents[0].Items.Count.ShouldBe(1);
        publishedEvents[0].Items[0].Name.ShouldBe("Milk");
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
