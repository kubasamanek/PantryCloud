using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Notification.Infrastructure.Persistence;

namespace PantryCloud.Notification.UnitTests;

internal static class TestHelper
{
    public static NotificationDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new NotificationDbContext(options);
    }

    public static ILogger<T> MockLogger<T>() where T : class
        => Substitute.For<ILogger<T>>();
}
