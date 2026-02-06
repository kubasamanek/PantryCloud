using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PantryCloud.SharedKernel.Entities;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Persistence;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Persistence;

public class AuditableEntityInterceptorTests
{

    [Fact]
    public async Task SavingChangesAsync_ShouldSetCreatedByAndCreatedAt_WhenEntityIsAdded()
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(Constants.Persistence.AuditUserId);

        await using var context = CreateContext(userContext);
        var entity = new TestAuditableEntity();
        context.TestEntities.Add(entity);

        await context.SaveChangesAsync();

        entity.CreatedBy.ShouldBe(Constants.Persistence.AuditUserId);
        entity.CreatedAt.ShouldNotBe(default);
        entity.ModifiedBy.ShouldBeNull();
        entity.ModifiedAt.ShouldBeNull();
    }

    [Fact]
    public async Task SavingChangesAsync_ShouldSetModifiedByAndModifiedAt_WhenEntityIsModified()
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(Constants.Persistence.AuditUserId);

        await using var context = CreateContext(userContext);
        var entity = new TestAuditableEntity { CreatedBy = Constants.Persistence.AuditUserId, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "Updated";
        await context.SaveChangesAsync();

        entity.ModifiedBy.ShouldBe(Constants.Persistence.AuditUserId);
        entity.ModifiedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task SavingChangesAsync_ShouldNotAccessUserId_WhenNoAuditableEntities()
    {
        var userContext = Substitute.For<IUserContext>();

        await using var context = CreateContextWithNonAuditableEntity(userContext);
        var entity = new TestNonAuditableEntity { Id = Guid.NewGuid() };
        context.NonAuditableEntities.Add(entity);

        await context.SaveChangesAsync();

        _ = userContext.DidNotReceive().UserId;
    }

    [Fact]
    public void SavingChanges_ShouldSetCreatedByAndCreatedAt_WhenEntityIsAdded()
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(Constants.Persistence.AuditUserId);

        using var context = CreateContext(userContext);
        var entity = new TestAuditableEntity();
        context.TestEntities.Add(entity);

        context.SaveChanges();

        entity.CreatedBy.ShouldBe(Constants.Persistence.AuditUserId);
        entity.CreatedAt.ShouldNotBe(default);
    }

    private static TestDbContext CreateContext(IUserContext userContext)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new AuditableEntityInterceptor(userContext))
            .Options;

        return new TestDbContext(options);
    }

    private static TestDbContextWithNonAuditable CreateContextWithNonAuditableEntity(IUserContext userContext)
    {
        var options = new DbContextOptionsBuilder<TestDbContextWithNonAuditable>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new AuditableEntityInterceptor(userContext))
            .Options;

        return new TestDbContextWithNonAuditable(options);
    }

    private class TestAuditableEntity : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private class TestNonAuditableEntity : IEntity
    {
        public Guid Id { get; set; }
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestAuditableEntity> TestEntities => Set<TestAuditableEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAuditableEntity>();
        }
    }

    private class TestDbContextWithNonAuditable : DbContext
    {
        public TestDbContextWithNonAuditable(DbContextOptions<TestDbContextWithNonAuditable> options) : base(options) { }

        public DbSet<TestNonAuditableEntity> NonAuditableEntities => Set<TestNonAuditableEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestNonAuditableEntity>(e =>
            {
                e.HasKey(x => x.Id);
            });
        }
    }
}
