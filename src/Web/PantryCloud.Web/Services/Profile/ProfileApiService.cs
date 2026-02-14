using System.Net.Http.Json;
using PantryCloud.Web.Constants;

namespace PantryCloud.Web.Services.Profile;

public class ProfileApiService(IHttpClientFactory httpClientFactory) : IProfileApi
{
    private static readonly string BasePath = ApiPaths.Households;
    private HttpClient Client => httpClientFactory.CreateClient("Gateway");

    public async Task<GetMyProfileResponse?> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync($"{BasePath}/me/profile", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GetMyProfileResponse>(cancellationToken)
            : null;
    }

    public async Task<UpdateProfileResponse?> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PutAsJsonAsync($"{BasePath}/me/profile", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<UpdateProfileResponse>(cancellationToken)
            : null;
    }
}
