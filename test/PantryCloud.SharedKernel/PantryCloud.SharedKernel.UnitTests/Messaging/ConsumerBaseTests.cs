using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.SharedKernel.Outbox;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Messaging;

internal class ConsumerBaseTestEvent : IntegrationEvent;

internal class ConsumerBaseTrackingConsumer(ConsumerBaseTestDbContext db)
    : DbContextConsumerBase<ConsumerBaseTestEvent, ConsumerBaseTestDbContext>(db, NullLogger<ConsumerBaseTrackingConsumer>.Instance)
{
    public int HandleCallCount { get; private set; }

    protected override Task HandleAsync(ConsumerBaseTestEvent @event, ConsumeContext context)
    {
        HandleCallCount++;
        return Task.CompletedTask;
    }
}

internal class ConsumerBaseThrowingConsumer(ConsumerBaseTestDbContext db)
    : DbContextConsumerBase<ConsumerBaseTestEvent, ConsumerBaseTestDbContext>(db, NullLogger<ConsumerBaseThrowingConsumer>.Instance)
{
    protected override Task HandleAsync(ConsumerBaseTestEvent @event, ConsumeContext context) =>
        throw new InvalidOperationException("Handler failed");
}

internal class ConsumerBaseTestDbContext(DbContextOptions<ConsumerBaseTestDbContext> options) : DbContext(options)
{
    public DbSet<ProcessedInboxMessage> ProcessedInboxMessages => Set<ProcessedInboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProcessedInboxMessageConfiguration());
    }
}

public class ConsumerBaseTests
{
    [Fact]
    public async Task Consume_ShouldCallHandleAsync_WhenMessageIsSeenForFirstTime()
    {
        await using var db = CreateContext();
        var consumer = new ConsumerBaseTrackingConsumer(db);
        var context = BuildContext(Guid.NewGuid(), new ConsumerBaseTestEvent());

        await consumer.Consume(context);

        consumer.HandleCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task Consume_ShouldSkipHandleAsync_WhenMessageIdAlreadyProcessed()
    {
        await using var db = CreateContext();
        var messageId = Guid.NewGuid();

        db.ProcessedInboxMessages.Add(new ProcessedInboxMessage
        {
            MessageId = messageId,
            EventType = nameof(ConsumerBaseTestEvent),
            ProcessedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var consumer = new ConsumerBaseTrackingConsumer(db);
        var context = BuildContext(messageId, new ConsumerBaseTestEvent());

        await consumer.Consume(context);

        consumer.HandleCallCount.ShouldBe(0);
    }

    [Fact]
    public async Task Consume_ShouldWriteProcessedInboxMessage_AfterSuccessfulHandle()
    {
        await using var db = CreateContext();
        var messageId = Guid.NewGuid();
        var consumer = new ConsumerBaseTrackingConsumer(db);
        var context = BuildContext(messageId, new ConsumerBaseTestEvent());

        await consumer.Consume(context);

        var inbox = await db.ProcessedInboxMessages.SingleAsync();
        inbox.MessageId.ShouldBe(messageId);
        inbox.EventType.ShouldContain(nameof(ConsumerBaseTestEvent));
    }

    [Fact]
    public async Task Consume_ShouldNotWriteProcessedInboxMessage_WhenMessageWasDuplicate()
    {
        await using var db = CreateContext();
        var messageId = Guid.NewGuid();

        db.ProcessedInboxMessages.Add(new ProcessedInboxMessage
        {
            MessageId = messageId,
            EventType = nameof(ConsumerBaseTestEvent),
            ProcessedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var consumer = new ConsumerBaseTrackingConsumer(db);
        var context = BuildContext(messageId, new ConsumerBaseTestEvent());

        await consumer.Consume(context);

        (await db.ProcessedInboxMessages.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task Consume_ShouldRethrowException_WhenHandlerThrows()
    {
        await using var db = CreateContext();
        var consumer = new ConsumerBaseThrowingConsumer(db);
        var context = BuildContext(Guid.NewGuid(), new ConsumerBaseTestEvent());

        await Should.ThrowAsync<InvalidOperationException>(() => consumer.Consume(context));
    }

    private static ConsumeContext<ConsumerBaseTestEvent> BuildContext(Guid messageId, ConsumerBaseTestEvent @event)
    {
        var context = Substitute.For<ConsumeContext<ConsumerBaseTestEvent>>();
        context.MessageId.Returns(messageId);
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);
        context.CorrelationId.Returns((Guid?)null);
        return context;
    }

    private static ConsumerBaseTestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ConsumerBaseTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
