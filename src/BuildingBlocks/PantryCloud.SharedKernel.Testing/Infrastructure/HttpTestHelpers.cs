using System.Text.Json;

namespace PantryCloud.SharedKernel.Testing.Infrastructure;

/// <summary>
/// Shared HTTP test helpers for integration tests.
/// </summary>
public static class HttpTestHelpers
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Deserializes JSON response to the specified type.
    /// </summary>
    public static async Task<T?> GetFromJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, DefaultOptions);
    }
}
