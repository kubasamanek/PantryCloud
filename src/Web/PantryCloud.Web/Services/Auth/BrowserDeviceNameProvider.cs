using Microsoft.JSInterop;

namespace PantryCloud.Web.Services.Auth;

public class BrowserDeviceNameProvider(IJSRuntime jsRuntime) : IDeviceNameProvider
{
    public async Task<string> GetDeviceNameAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var name = await jsRuntime.InvokeAsync<string>("getBrowserDescription", cancellationToken);
            return string.IsNullOrWhiteSpace(name) ? "Web browser" : name.Trim();
        }
        catch
        {
            return "Web browser";
        }
    }
}
