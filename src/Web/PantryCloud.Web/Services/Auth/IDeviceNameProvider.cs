namespace PantryCloud.Web.Services.Auth;

/// <summary>
/// Provides a short description of the current browser/device for session display (e.g. "Chrome on Windows").
/// </summary>
public interface IDeviceNameProvider
{
    Task<string> GetDeviceNameAsync(CancellationToken cancellationToken = default);
}
