using System.Net.Http.Headers;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using PantryCloud.Pantry.Infrastructure.Persistence;
using PantryCloud.Pantry.IntegrationTests.Constants;
using PantryCloud.Pantry.IntegrationTests.Infrastructure.Containers;
using PantryCloud.Pantry.IntegrationTests.Infrastructure.Images;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.SharedKernel.Testing.Infrastructure.Environment;
using PantryCloud.SharedKernel.Testing.Infrastructure.RabbitMq;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace PantryCloud.Pantry.IntegrationTests.Infrastructure;

public sealed class PantryTestFixture : IAsyncLifetime
{
    private static readonly string TestJwtSecret = TestJwtProvider.GenerateSecret();

    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgres;
    private readonly RabbitMqContainer _rabbitMq;
    private readonly IFutureDockerImage _pantryImage;
    private readonly IContainer _pantryApi;
    private readonly TestJwtProvider _jwtProvider;
    private Respawner _respawner = null!;

    private string ConnectionString => $"Host=127.0.0.1;Port={_postgres.GetMappedPublicPort(IntegrationConstants.Postgres.Port)};Database={IntegrationConstants.Postgres.PantryDatabase};Username={IntegrationConstants.Postgres.User};Password={IntegrationConstants.Postgres.Password}";

    public PantryTestFixture()
    {
        _network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();

        _postgres = PostgresContainer.Create(_network).Build();
        _rabbitMq = RabbitMqContainerBuilder.Create(_network).Build();
        _pantryImage = PantryImageBuilder.Build();
        _pantryApi = PantryContainer.Create(
            _pantryImage,
            _network,
            $"Host={IntegrationConstants.Postgres.Host};Port={IntegrationConstants.Postgres.Port};Database={IntegrationConstants.Postgres.PantryDatabase};Username={IntegrationConstants.Postgres.User};Password={IntegrationConstants.Postgres.Password}",
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

        await PostgresDatabaseSetup.CreateDatabaseAsync(_postgres, IntegrationConstants.Postgres.PantryDatabase, IntegrationConstants.Postgres.User, IntegrationConstants.Postgres.DefaultDatabase);
        await PostgresDatabaseSetup.ApplyMigrationsAsync<PantryDbContext>(_postgres, IntegrationConstants.Postgres.PantryDatabase, IntegrationConstants.Postgres.User, IntegrationConstants.Postgres.Password, IntegrationConstants.Postgres.Port);

        await _rabbitMq.StartAsync();
        await _pantryImage.CreateAsync();
        await _pantryApi.StartAsync();

        BaseAddress = $"http://127.0.0.1:{_pantryApi.GetMappedPublicPort(IntegrationConstants.Pantry.Port)}/";
        HttpClient = new HttpClient { BaseAddress = new Uri(BaseAddress) };

        using (var warmup = CreateClientWithToken(Guid.NewGuid()))
        {
            _ = await warmup.GetAsync(TestConstants.Endpoints.ListItems);
        }

        await InitializeRespawnerAsync();
    }

    private async Task InitializeRespawnerAsync()
    {
        var options = new DbContextOptionsBuilder<PantryDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var context = new PantryDbContext(options);
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task ResetAsync()
    {
        var options = new DbContextOptionsBuilder<PantryDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var context = new PantryDbContext(options);
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Seeds a household membership so the user can call pantry APIs (which require current household).
    /// Call after ResetAsync when the test uses a specific user.
    /// </summary>
    public async Task SeedHouseholdMembershipAsync(Guid userId, Guid householdId)
    {
        var options = new DbContextOptionsBuilder<PantryDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var context = new PantryDbContext(options);
        var existing = await context.UserHouseholdMemberships.FindAsync(userId);
        if (existing != null)
        {
            existing.HouseholdId = householdId;
            existing.LeftAt = null;
            existing.JoinedAt = DateTime.UtcNow;
        }
        else
        {
            context.UserHouseholdMemberships.Add(new UserHouseholdMembership
            {
                UserId = userId,
                HouseholdId = householdId,
                JoinedAt = DateTime.UtcNow,
                LeftAt = null
            });
        }
        await context.SaveChangesAsync();
    }

    public HttpClient CreateClient() => new() { BaseAddress = new Uri(BaseAddress) };

    public HttpClient CreateClientWithToken(Guid userId, string? email = null)
    {
        var client = new HttpClient { BaseAddress = new Uri(BaseAddress) };
        var token = _jwtProvider.CreateToken(userId, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task DisposeAsync()
    {
        await _pantryApi.DisposeAsync();
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
