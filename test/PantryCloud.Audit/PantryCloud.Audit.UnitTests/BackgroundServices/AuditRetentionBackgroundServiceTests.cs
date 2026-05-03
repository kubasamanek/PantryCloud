using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using PantryCloud.Audit.Core.Options;
using PantryCloud.Audit.Infrastructure.BackgroundServices;
using PantryCloud.Audit.Infrastructure.Persistence;
using Shouldly;

namespace PantryCloud.Audit.UnitTests.BackgroundServices;

public class AuditRetentionBackgroundServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogStarted_WhenServiceStarts()
    {
        var scopeFactory = CreateScopeFactoryWithDbContext();
        var options = Options.Create(new AuditOptions { RetentionDays = 30 });
        var logger = Substitute.For<ILogger<AuditRetentionBackgroundService>>();

        var service = new AuditRetentionBackgroundService(scopeFactory, options, logger);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        var startTask = service.StartAsync(cts.Token);

        await Task.WhenAny(startTask, Task.Delay(200));
        await service.StopAsync(CancellationToken.None);

        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("started")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    private static IServiceScopeFactory CreateScopeFactoryWithDbContext()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AuditDbContext>(opts =>
            opts.UseInMemoryDatabase("AuditRetentionTest_" + Guid.NewGuid()));
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }
}
