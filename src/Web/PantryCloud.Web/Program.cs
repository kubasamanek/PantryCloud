using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PantryCloud.Web;
using PantryCloud.Web.Services;
using PantryCloud.Web.Services.Auth;
using PantryCloud.Web.Services.Sessions;

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

// Auth API (no Bearer) for login/register/refresh/forgot/reset
builder.Services.AddHttpClient("Auth", (sp, client) =>
{
    client.BaseAddress = gatewayUri;
});
builder.Services.AddScoped<IDeviceNameProvider, BrowserDeviceNameProvider>();
builder.Services.AddScoped<IAuthApi, AuthApiService>();
builder.Services.AddScoped<ISessionsApi, SessionsApiService>();

// Gateway client with Bearer + 401 refresh
builder.Services.AddScoped<GatewayAuthorizationMessageHandler>();
builder.Services.AddHttpClient("Gateway", (sp, client) =>
{
    client.BaseAddress = gatewayUri;
}).AddHttpMessageHandler<GatewayAuthorizationMessageHandler>();

await builder.Build().RunAsync();
