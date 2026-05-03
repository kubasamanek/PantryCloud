namespace PantryCloud.Web.Services.Profile;

public interface IProfileApi
{
    Task<GetMyProfileResponse?> GetMyProfileAsync(CancellationToken cancellationToken = default);
    Task<UpdateProfileResponse?> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
}
