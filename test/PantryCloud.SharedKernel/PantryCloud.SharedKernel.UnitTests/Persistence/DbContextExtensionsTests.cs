using Microsoft.EntityFrameworkCore;
using PantryCloud.SharedKernel.Entities;
using PantryCloud.SharedKernel.Persistence;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Persistence;

public class DbContextExtensionsTests
{

    [Fact]
    public async Task FindByIdAsync_ShouldReturnEntity_WhenFound()
    {
        await using var context = CreateContext();
        context.TestEntities.Add(new TestEntity { Id = Constants.Persistence.ExistingEntityId, Name = Constants.Persistence.TestEntityName });
        await context.SaveChangesAsync();

        var result = await context.FindByIdAsync<TestEntity>(Constants.Persistence.ExistingEntityId);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(Constants.Persistence.ExistingEntityId);
        result.Name.ShouldBe(Constants.Persistence.TestEntityName);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var context = CreateContext();

        var result = await context.FindByIdAsync<TestEntity>(Constants.Persistence.NonExistentEntityId);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenEntityExists()
    {
        await using var context = CreateContext();
        context.TestEntities.Add(new TestEntity { Id = Constants.Persistence.ExistingEntityId, Name = Constants.Persistence.TestEntityName });
        await context.SaveChangesAsync();

        var result = await context.ExistsAsync<TestEntity>(Constants.Persistence.ExistingEntityId);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        await using var context = CreateContext();

        var result = await context.ExistsAsync<TestEntity>(Constants.Persistence.NonExistentEntityId);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task FindByIdAsync_ShouldPassCancellationToken()
    {
        await using var context = CreateContext();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(
            () => context.FindByIdAsync<TestEntity>(Constants.Persistence.ExistingEntityId, cts.Token));
    }

    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    private class TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(e =>
            {
                e.HasKey(x => x.Id);
            });
        }
    }
}
