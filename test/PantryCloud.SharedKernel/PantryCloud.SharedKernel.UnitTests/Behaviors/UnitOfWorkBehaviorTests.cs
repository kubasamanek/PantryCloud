using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PantryCloud.SharedKernel.Behaviors;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.SharedKernel.Outbox;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Behaviors;

public class UnitOfWorkBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldSaveChanges_WhenHandlerSucceedsAndChangesExist()
    {
        await using var db = CreateContext();
        var behavior = CreateBehavior(db);
        var writer = new OutboxWriter<TestDbContext>(db, NullLogger<OutboxWriter<TestDbContext>>.Instance);
        ErrorOr<string> handlerResponse = Constants.Outbox.SuccessValue;

        var result = await behavior.Handle(new TestCommand(), Next, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(Constants.Outbox.SuccessValue);
        (await db.Set<OutboxMessage>().CountAsync()).ShouldBe(1);
        return;

        async Task<ErrorOr<string>> Next(CancellationToken ct)
        {
            await writer.WriteAsync(new TestIntegrationEvent { Data = Constants.Outbox.SuccessValue }, ct);
            return handlerResponse;
        }
    }

    [Fact]
    public async Task Handle_ShouldNotSaveChanges_WhenHandlerReturnsError()
    {
        await using var db = CreateContext();
        var behavior = CreateBehavior(db);
        var writer = new OutboxWriter<TestDbContext>(db, NullLogger<OutboxWriter<TestDbContext>>.Instance);

        var result = await behavior.Handle(new TestCommand(), Next, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        (await db.Set<OutboxMessage>().CountAsync()).ShouldBe(0);
        return;

        async Task<ErrorOr<string>> Next(CancellationToken ct)
        {
            await writer.WriteAsync(new TestIntegrationEvent { Data = "should-not-persist" }, ct);
            return Error.Failure("Test.Error", "something failed");
        }
    }

    [Fact]
    public async Task Handle_ShouldSkipSave_WhenNoPendingChangesExist()
    {
        await using var db = CreateContext();
        var behavior = CreateBehavior(db);
        ErrorOr<string> handlerResponse = Constants.Outbox.SuccessValue;

        var saveCountBefore = db.SaveChangesCallCount;

        await behavior.Handle(new TestCommand(), Next, CancellationToken.None);

        db.SaveChangesCallCount.ShouldBe(saveCountBefore);
        return;

        Task<ErrorOr<string>> Next(CancellationToken ct) => Task.FromResult(handlerResponse);
    }

    [Fact]
    public async Task Handle_ShouldReturnOriginalResponse_WhenHandlerSucceeds()
    {
        await using var db = CreateContext();
        var behavior = CreateBehavior(db);
        ErrorOr<string> handlerResponse = Constants.Outbox.SuccessValue;

        var result = await behavior.Handle(new TestCommand(), Next, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(Constants.Outbox.SuccessValue);
        return;

        Task<ErrorOr<string>> Next(CancellationToken ct) => Task.FromResult(handlerResponse);
    }

    [Fact]
    public async Task Handle_ShouldReturnOriginalErrors_WhenHandlerFails()
    {
        await using var db = CreateContext();
        var behavior = CreateBehavior(db);
        var error = Error.NotFound("Test.NotFound", "entity not found");

        var result = await behavior.Handle(new TestCommand(), Next, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Test.NotFound");
        return;

        Task<ErrorOr<string>> Next(CancellationToken ct)
        {
            ErrorOr<string> errorResult = error;
            return Task.FromResult(errorResult);
        }
    }

    [Fact]
    public async Task Handle_ShouldSaveMultipleOutboxMessages_InSingleCommit()
    {
        await using var db = CreateContext();
        var behavior = CreateBehavior(db);
        var writer = new OutboxWriter<TestDbContext>(db, NullLogger<OutboxWriter<TestDbContext>>.Instance);
        ErrorOr<string> handlerResponse = Constants.Outbox.SuccessValue;

        await behavior.Handle(new TestCommand(), Next, CancellationToken.None);

        (await db.Set<OutboxMessage>().CountAsync()).ShouldBe(2);
        return;

        async Task<ErrorOr<string>> Next(CancellationToken ct)
        {
            await writer.WriteAsync(new TestIntegrationEvent { Data = "first" }, ct);
            await writer.WriteAsync(new TestIntegrationEvent { Data = "second" }, ct);
            return handlerResponse;
        }
    }

    private static UnitOfWorkBehavior<TestCommand, ErrorOr<string>> CreateBehavior(DbContext db) =>
        new(db, NullLogger<UnitOfWorkBehavior<TestCommand, ErrorOr<string>>>.Instance);

    private static TestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private record TestCommand : IRequest<ErrorOr<string>>;

    private class TestIntegrationEvent : IntegrationEvent
    {
        public string Data { get; init; } = string.Empty;
    }

    public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public int SaveChangesCallCount { get; private set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        }
    }
}
