using System.Net.Http.Headers;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using PantryCloud.Household.Infrastructure.Persistence;
using PantryCloud.Household.IntegrationTests.Constants;
using PantryCloud.Household.IntegrationTests.Infrastructure.Containers;
using PantryCloud.Household.IntegrationTests.Infrastructure.Images;
using PantryCloud.SharedKernel.Testing.Infrastructure.Environment;
using PantryCloud.SharedKernel.Testing.Infrastructure.RabbitMq;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace PantryCloud.Household.IntegrationTests.Infrastructure;

/// <summary>
/// Shared fixture for Household integration tests. Runs the Household API in a container
/// with real Postgres and RabbitMQ. Provides HTTP clients with JWT and Respawn for DB reset.
/// </summary>
public sealed class HouseholdTestFixture : IAsyncLifetime
{
    private static readonly string TestJwtSecret = TestJwtProvider.GenerateSecret();

    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgres;
    private readonly RabbitMqContainer _rabbitMq;
    private readonly IFutureDockerImage _householdImage;
    private readonly IContainer _householdApi;
    private readonly TestJwtProvider _jwtProvider;
    private Respawner _respawner = null!;

    private string ConnectionString => $"Host=127.0.0.1;Port={_postgres.GetMappedPublicPort(IntegrationConstants.Postgres.Port)};Database={IntegrationConstants.Postgres.HouseholdDatabase};Username={IntegrationConstants.Postgres.User};Password={IntegrationConstants.Postgres.Password}";

    public HouseholdTestFixture()
    {
        _network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();

        _postgres = PostgresContainerBuilder.Create(_network).Build();
        _rabbitMq = RabbitMqContainerBuilder.Create(_network).Build();
        _householdImage = HouseholdImageBuilder.Build();
        _householdApi = HouseholdContainer.Create(
            _householdImage,
            _network,
            $"Host={IntegrationConstants.Postgres.Host};Port={IntegrationConstants.Postgres.Port};Database={IntegrationConstants.Postgres.HouseholdDatabase};Username={IntegrationConstants.Postgres.User};Password={IntegrationConstants.Postgres.Password}",
            "rabbitmq",
            TestJwtSecret
        ).Build();
        _jwtProvider = new TestJwtProvider(TestJwtSecret);
    }

    public HttpClient HttpClient { get; private set; } = null!;
    private string BaseAddress { get; set; } = null!;

    public async Task InitializeAsync()
    {
        await _network.CreateAsync();
        await _postgres.StartAsync();

        await PostgresDatabaseSetup.CreateDatabaseAsync(_postgres, IntegrationConstants.Postgres.HouseholdDatabase, IntegrationConstants.Postgres.User, IntegrationConstants.Postgres.DefaultDatabase);
        await PostgresDatabaseSetup.ApplyMigrationsAsync<HouseholdDbContext>(_postgres, IntegrationConstants.Postgres.HouseholdDatabase, IntegrationConstants.Postgres.User, IntegrationConstants.Postgres.Password, IntegrationConstants.Postgres.Port);

        await _rabbitMq.StartAsync();
        await _householdImage.CreateAsync();
        await _householdApi.StartAsync();

        BaseAddress = $"http://127.0.0.1:{_householdApi.GetMappedPublicPort(IntegrationConstants.Household.Port)}/";
        HttpClient = new HttpClient { BaseAddress = new Uri(BaseAddress) };

        // Warm up JWT pipeline so the first test request is not rejected (container can be slow to accept tokens).
        using (var warmup = CreateClientWithToken(Guid.NewGuid()))
        {
            _ = await warmup.GetAsync(TestConstants.Endpoints.GetCurrentHousehold);
        }

        await InitializeRespawnerAsync();
    }

    private async Task InitializeRespawnerAsync()
    {
        var options = new DbContextOptionsBuilder<HouseholdDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var context = new HouseholdDbContext(options);
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    /// <summary>
    /// Resets the database to a clean state. Call at the start of each test for isolation.
    /// </summary>
    public async Task ResetAsync()
    {
        var options = new DbContextOptionsBuilder<HouseholdDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var context = new HouseholdDbContext(options);
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Creates an HTTP client with no authentication (for testing 401).
    /// </summary>
    public HttpClient CreateClient() => new() { BaseAddress = new Uri(BaseAddress) };

    /// <summary>
    /// Creates an HTTP client with Bearer token for the given user. Pass <paramref name="email"/> when the
    /// test requires a specific email (e.g. invitee must have the same email as the invitation to join).
    /// </summary>
    public HttpClient CreateClientWithToken(Guid userId, string? email = null)
    {
        var client = new HttpClient { BaseAddress = new Uri(BaseAddress) };
        var token = _jwtProvider.CreateToken(userId, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
    
    public RabbitMqPublisher CreateRabbitMqPublisher() =>
        RabbitMqPublisher.CreateFromHostPort($"localhost:{_rabbitMq.GetMappedPublicPort(RabbitMqTestOptions.AmqpPort)}");

    public async Task DisposeAsync()
    {
        await _householdApi.DisposeAsync();
        await _rabbitMq.DisposeAsync();
        await _postgres.DisposeAsync();
        await _network.DisposeAsync();
    }

    public static async Task<T?> GetFromJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
