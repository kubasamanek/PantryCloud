using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.SharedKernel.Outbox;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Messaging;

public class OutboxWriterTests
{
    [Fact]
    public async Task WriteAsync_ShouldAddOutboxMessageToChangeTracker_WithoutSaving()
    {
        await using var db = CreateContext();
        var writer = new OutboxWriter<TestDbContext>(db, NullLogger<OutboxWriter<TestDbContext>>.Instance);

        await writer.WriteAsync(new TestIntegrationEvent { Data = Constants.Outbox.SuccessValue });

        db.ChangeTracker.Entries<OutboxMessage>().ShouldHaveSingleItem();
        db.ChangeTracker.Entries<OutboxMessage>().Single().State.ShouldBe(EntityState.Added);

        var pending = db.ChangeTracker.Entries<OutboxMessage>().Single().Entity;
        pending.ProcessedAt.ShouldBeNull();
        pending.EventType.ShouldContain(nameof(TestIntegrationEvent));
        pending.Payload.ShouldContain(Constants.Outbox.SuccessValue);
    }

    [Fact]
    public async Task WriteAsync_ShouldPersistOutboxMessage_WhenSaveChangesIsCalled()
    {
        await using var db = CreateContext();
        var writer = new OutboxWriter<TestDbContext>(db, NullLogger<OutboxWriter<TestDbContext>>.Instance);

        await writer.WriteAsync(new TestIntegrationEvent { Data = Constants.Outbox.SuccessValue });
        await db.SaveChangesAsync();

        var saved = await db.Set<OutboxMessage>().SingleAsync();
        saved.ProcessedAt.ShouldBeNull();
        saved.EventType.ShouldContain(nameof(TestIntegrationEvent));
        saved.OccurredAt.ShouldNotBe(default);
    }

    [Fact]
    public async Task WriteAsync_ShouldSupportMultipleEventsInSameContext()
    {
        await using var db = CreateContext();
        var writer = new OutboxWriter<TestDbContext>(db, NullLogger<OutboxWriter<TestDbContext>>.Instance);

        await writer.WriteAsync(new TestIntegrationEvent { Data = "first" });
        await writer.WriteAsync(new TestIntegrationEvent { Data = "second" });
        await db.SaveChangesAsync();

        var messages = await db.Set<OutboxMessage>().ToListAsync();
        messages.Count.ShouldBe(2);
    }

    private static TestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private class TestIntegrationEvent : IntegrationEvent
    {
        public string Data { get; init; } = string.Empty;
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        }
    }
}
