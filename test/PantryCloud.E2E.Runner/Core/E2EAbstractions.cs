using PantryCloud.E2E.Runner.Configuration;
using PantryCloud.E2E.Runner.Http;
using PantryCloud.E2E.Runner.HttpClients;

namespace PantryCloud.E2E.Runner.Core;

public interface IE2EScenario
{
    string Name { get; }

    Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken);
}

public sealed record E2EScenarioResult(bool Success, string? Error = null);

public sealed class E2EContext
{
    public E2EContext(E2ESettings settings)
    {
        Settings = settings;
        Client = E2EHttpClientFactory.Create(settings.Gateway);
        Auth = new AuthApi(Client, settings);
        Household = new HouseholdApi(Client, settings);
        Pantry = new PantryApi(Client, settings);
        Recipe = new RecipeApi(Client, settings);
        ShoppingList = new ShoppingListApi(Client, settings);
        Notification = new NotificationApi(Client, settings);
        Audit = new AuditApi(Client, settings);
        Random = new Random();
    }

    public E2ESettings Settings { get; }

    public HttpClient Client { get; }

    public AuthApi Auth { get; }

    public HouseholdApi Household { get; }

    public PantryApi Pantry { get; }

    public RecipeApi Recipe { get; }

    public ShoppingListApi ShoppingList { get; }

    public NotificationApi Notification { get; }

    public AuditApi Audit { get; }

    public Random Random { get; }
}

