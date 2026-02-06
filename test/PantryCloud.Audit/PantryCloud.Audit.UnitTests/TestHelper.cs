using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Audit.Infrastructure.Persistence;

namespace PantryCloud.Audit.UnitTests;

internal static class TestHelper
{
    public static AuditDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseInMemoryDatabase(dbName + "_" + Guid.NewGuid())
            .Options;
        return new AuditDbContext(options);
    }

    public static ILogger<T> MockLogger<T>() where T : class
        => Substitute.For<ILogger<T>>();
}
