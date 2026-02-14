using Microsoft.AspNetCore.Components;

namespace PantryCloud.Web.Helpers;

/// <summary>
/// Helpers for navigation and URL parsing.
/// </summary>
public static class NavigationHelper
{
    private const string ReturnUrlQueryPrefix = "returnUrl=";

    /// <summary>
    /// Gets the return URL from the current URI query string, or the default path if missing or invalid.
    /// </summary>
    /// <param name="navigation">The NavigationManager.</param>
    /// <param name="defaultPath">Default path to use when no valid returnUrl is present (e.g. "/household").</param>
    /// <returns>A path to navigate to after login.</returns>
    public static string GetReturnUrlOrDefault(NavigationManager navigation, string defaultPath)
    {
        var uri = navigation.Uri;
        var idx = uri.IndexOf(ReturnUrlQueryPrefix, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return defaultPath;

        idx += ReturnUrlQueryPrefix.Length;
        var segment = uri[idx..].Split('&')[0];
        if (string.IsNullOrWhiteSpace(segment))
            return defaultPath;

        var decoded = Uri.UnescapeDataString(segment.Trim());
        if (decoded.StartsWith("/", StringComparison.Ordinal) || decoded.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return decoded;

        return defaultPath;
    }
}
