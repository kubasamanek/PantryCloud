using System.Net.Http.Headers;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using PantryCloud.Recipe.Application.Dtos;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using PantryCloud.Recipe.IntegrationTests.Constants;
using PantryCloud.Recipe.IntegrationTests.Infrastructure.Containers;
using PantryCloud.Recipe.IntegrationTests.Infrastructure.Images;
using PantryCloud.SharedKernel.Testing.Infrastructure.Environment;
using Testcontainers.MongoDb;

namespace PantryCloud.Recipe.IntegrationTests.Infrastructure;

public sealed class RecipeTestFixture : IAsyncLifetime
{
    private static readonly string TestJwtSecret = TestJwtProvider.GenerateSecret();

    private readonly INetwork _network;
    private readonly MongoDbContainer _mongo;
    private readonly IFutureDockerImage _recipeImage;
    private readonly IContainer _recipeApi;
    private readonly TestJwtProvider _jwtProvider;

    public RecipeTestFixture()
    {
        _network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();

        _mongo = MongoContainer.Create(_network);
        _recipeImage = RecipeImageBuilder.Build();
        var mongoConnectionString = $"mongodb://{IntegrationConstants.Mongo.Host}:{IntegrationConstants.Mongo.Port}/";
        _recipeApi = RecipeContainer.Create(
            _recipeImage,
            _network,
            mongoConnectionString,
            TestJwtSecret
        ).Build();
        _jwtProvider = new TestJwtProvider(TestJwtSecret);
    }

    public HttpClient HttpClient { get; private set; } = null!;
    private string BaseAddress { get; set; } = null!;

    public async Task InitializeAsync()
    {
        await _network.CreateAsync();
        await _mongo.StartAsync();
        await _recipeImage.CreateAsync();
        await _recipeApi.StartAsync();

        BaseAddress = $"http://127.0.0.1:{_recipeApi.GetMappedPublicPort(IntegrationConstants.Recipe.Port)}/";
        HttpClient = new HttpClient { BaseAddress = new Uri(BaseAddress) };

        // Warm up the API with a valid authenticated request (valid search body: at least one ingredient)
        using var warmup = CreateClientWithToken(Guid.NewGuid());
        var warmupBody = JsonSerializer.Serialize(new SearchRecipesRequestDto(new List<string> { "warmup" }));
        _ = await warmup.PostAsync(TestConstants.Endpoints.Search, new StringContent(warmupBody, System.Text.Encoding.UTF8, "application/json"));
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
        await _recipeApi.DisposeAsync();
        await _mongo.DisposeAsync();
        await _network.DisposeAsync();
    }

    public static async Task<T?> GetFromJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
