using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using PantryCloud.Notification.Core.Entities;
using PantryCloud.Notification.Infrastructure.Persistence;
using PantryCloud.Notification.IntegrationTests.Infrastructure.Containers;
using PantryCloud.Notification.IntegrationTests.Infrastructure.Environment;
using PantryCloud.Notification.IntegrationTests.Infrastructure.Images;
using PantryCloud.SharedKernel.Testing.Infrastructure.Environment;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace PantryCloud.Notification.IntegrationTests.Infrastructure;

public sealed class NotificationTestFixture : IAsyncLifetime
{
    private static readonly string TestJwtSecret = TestJwtProvider.GenerateSecret();

    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgres;
    private readonly RabbitMqContainer _rabbitMq;
    private readonly IFutureDockerImage _notificationImage;
    private readonly IContainer _notificationApi;
    private readonly TestJwtProvider _jwtProvider;

    private string NotificationBaseUrl => $"http://{_notificationApi.Hostname}:{_notificationApi.GetMappedPublicPort(Constants.Notification.Port)}";
    private string RabbitMqHostPort => $"{_rabbitMq.Hostname}:{_rabbitMq.GetMappedPublicPort(Constants.RabbitMq.AmqpPort)}";

    public NotificationTestFixture()
    {
        _network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();

        _postgres = PostgresContainer.Create(_network).Build();
        _rabbitMq = RabbitMqContainerConfig.Create(_network).Build();
        _notificationImage = NotificationImageBuilder.Build();
        _notificationApi = NotificationContainer.Create(
            _notificationImage,
            _network,
            $"Host={Constants.Postgres.Host};Port={Constants.Postgres.Port};Database={Constants.Postgres.NotificationDatabase};Username={Constants.Postgres.User};Password={Constants.Postgres.Password}",
            Constants.RabbitMq.Host,
            TestJwtSecret
        ).Build();
        _jwtProvider = new TestJwtProvider(TestJwtSecret);
    }

    public async Task InitializeAsync()
    {
        await _network.CreateAsync();
        await _postgres.StartAsync();

        await PostgresDatabaseSetup.CreateDatabaseAsync(_postgres, Constants.Postgres.NotificationDatabase, Constants.Postgres.User, Constants.Postgres.DefaultDatabase);
        await PostgresDatabaseSetup.ApplyMigrationsAsync<NotificationDbContext>(_postgres, Constants.Postgres.NotificationDatabase, Constants.Postgres.User, Constants.Postgres.Password, Constants.Postgres.Port);
        await _rabbitMq.StartAsync();

        await _notificationImage.CreateAsync();
        await _notificationApi.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _notificationApi.DisposeAsync();
        await _rabbitMq.DisposeAsync();
        await _postgres.DisposeAsync();
        await _network.DisposeAsync();
    }

    public (string AccessToken, Guid UserId) CreateTestUser()
    {
        var userId = Guid.NewGuid();
        return (_jwtProvider.CreateToken(userId), userId);
    }

    public async Task SeedHouseholdMembershipAsync(Guid userId, Guid householdId)
    {
        var connStr = $"Host={_postgres.Hostname};Port={_postgres.GetMappedPublicPort(Constants.Postgres.Port)};Database={Constants.Postgres.NotificationDatabase};Username={Constants.Postgres.User};Password={Constants.Postgres.Password}";
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql(connStr)
            .Options;
        await using var context = new NotificationDbContext(options);
        context.UserHouseholdMemberships.Add(new UserHouseholdMembership
        {
            UserId = userId,
            HouseholdId = householdId,
            JoinedAt = DateTime.UtcNow.AddHours(-1)
        });
        await context.SaveChangesAsync();
    }

    public async Task<HubConnection> CreateSignalRConnectionAsync(string accessToken)
    {
        var url = $"{NotificationBaseUrl}{Constants.Notification.HubPath}?access_token={Uri.EscapeDataString(accessToken)}";
        var connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();
        await connection.StartAsync();
        return connection;
    }

    public async Task PublishEventAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        await using var publisher = new EventPublisher(RabbitMqHostPort);
        await publisher.PublishAsync(message, cancellationToken);
    }
}
