using Microsoft.JSInterop;

namespace PantryCloud.Web.Services.Auth;

public class BrowserDeviceNameProvider : IDeviceNameProvider
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserDeviceNameProvider(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> GetDeviceNameAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var name = await _jsRuntime.InvokeAsync<string>("getBrowserDescription", cancellationToken);
            return string.IsNullOrWhiteSpace(name) ? "Web browser" : name.Trim();
        }
        catch
        {
            return "Web browser";
        }
    }
}
