using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PantryCloud.SharedKernel.Identity;
using PantryCloud.SharedKernel.Services;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Services;

public class BaseServiceTests
{
    [Fact]
    public void UserId_ShouldReturnValueFromUserContext()
    {
        var expectedUserId = Guid.NewGuid();
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(expectedUserId);

        var service = new ConcreteService(userContext);

        service.ExposedUserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void UserEmail_ShouldReturnValueFromUserContext()
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.Email.Returns(Constants.Identity.TestEmail);

        var service = new ConcreteService(userContext);

        service.ExposedUserEmail.ShouldBe(Constants.Identity.TestEmail);
    }

    [Fact]
    public void Logger_ShouldBeSetFromConstructor()
    {
        var userContext = Substitute.For<IUserContext>();

        var service = new ConcreteService(userContext);

        service.ExposedLogger.ShouldNotBeNull();
    }

    private class ConcreteService(IUserContext userContext)
        : BaseService<ConcreteService>(userContext, NullLogger<ConcreteService>.Instance)
    {
        public Guid ExposedUserId => UserId;
        public string ExposedUserEmail => UserEmail;
        public object ExposedLogger => Logger;
    }
}

public class BaseDbContextServiceTests
{
    [Fact]
    public void DbContext_ShouldBeSetFromConstructor()
    {
        var userContext = Substitute.For<IUserContext>();
        var options = new DbContextOptionsBuilder<TestServiceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new TestServiceDbContext(options);

        var service = new ConcreteDbContextService(db, userContext);

        service.ExposedDbContext.ShouldBe(db);
    }

    [Fact]
    public void InheritedUserId_ShouldWorkThroughBaseService()
    {
        var userId = Guid.NewGuid();
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(userId);
        var options = new DbContextOptionsBuilder<TestServiceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new TestServiceDbContext(options);

        var service = new ConcreteDbContextService(db, userContext);

        service.ExposedUserId.ShouldBe(userId);
    }

    private class ConcreteDbContextService(TestServiceDbContext dbContext, IUserContext userContext)
        : BaseDbContextService<ConcreteDbContextService, TestServiceDbContext>(dbContext, userContext,
            NullLogger<ConcreteDbContextService>.Instance)
    {
        public TestServiceDbContext ExposedDbContext => DbContext;
        public Guid ExposedUserId => UserId;
    }

    internal class TestServiceDbContext(DbContextOptions<TestServiceDbContext> options) : DbContext(options);
}
