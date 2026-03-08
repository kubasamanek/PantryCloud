using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PantryCloud.Web;
using PantryCloud.Web.Services;
using PantryCloud.Web.Services.Auth;
using PantryCloud.Web.Services.Household;
using PantryCloud.Web.Services.Pantry;
using PantryCloud.Web.Services.Profile;
using PantryCloud.Web.Services.Notification;
using PantryCloud.Web.Services.Recipe;
using PantryCloud.Web.Services.ShoppingList;
using PantryCloud.Web.Services.Sessions;
using PantryCloud.Web.Services.Audit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();
builder.Services.Configure<GatewayOptions>(
    builder.Configuration.GetSection("Gateway"));

var gatewayBaseUrl = builder.Configuration.GetValue<string>("Gateway:BaseUrl") ?? "http://localhost:5050";
var gatewayUri = new Uri(gatewayBaseUrl.TrimEnd('/') + "/");

// Token storage and auth state
builder.Services.AddSingleton<ITokenStorage, LocalStorageTokenStorage>();
builder.Services.AddScoped<JwtClaimsHelper>();
builder.Services.AddScoped<AuthenticationStateProvider, TokenAuthenticationStateProvider>();
builder.Services.AddScoped(sp => (TokenAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

// Auth API (no Bearer)
builder.Services.AddHttpClient("Auth", (_, client) =>
{
    client.BaseAddress = gatewayUri;
});

builder.Services.AddScoped<IDeviceNameProvider, BrowserDeviceNameProvider>();
builder.Services.AddScoped<IAuthApi, AuthApiService>();
builder.Services.AddScoped<ISessionsApi, SessionsApiService>();
builder.Services.AddScoped<IProfileApi, ProfileApiService>();
builder.Services.AddScoped<ICurrentProfileService, CurrentProfileService>();
builder.Services.AddScoped<IHouseholdApi, HouseholdApiService>();
builder.Services.AddScoped<IPantryApi, PantryApiService>();
builder.Services.AddScoped<IShoppingListApi, ShoppingListApiService>();
builder.Services.AddScoped<IRecipeApi, RecipeApiService>();
builder.Services.AddScoped<INotificationHubClient, NotificationHubClient>();
builder.Services.AddScoped<INotificationStateService, NotificationStateService>();
builder.Services.AddScoped<INotificationApi, NotificationApiService>();
builder.Services.AddScoped<IAuditApi, AuditApiService>();

// Gateway client with Bearer + 401 refresh
builder.Services.AddScoped<GatewayAuthorizationMessageHandler>();
builder.Services.AddHttpClient("Gateway", (sp, client) =>
{
    client.BaseAddress = gatewayUri;
}).AddHttpMessageHandler<GatewayAuthorizationMessageHandler>();

await builder.Build().RunAsync();
