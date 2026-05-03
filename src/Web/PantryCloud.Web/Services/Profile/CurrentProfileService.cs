namespace PantryCloud.Web.Services.Profile;

public class CurrentProfileService(IProfileApi profileApi) : ICurrentProfileService
{
    public string? DisplayName { get; private set; }

    public event Action? ProfileUpdated;

    private bool _loaded;

    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (_loaded)
        {
            return;
        }

        await LoadAsync(cancellationToken);
        _loaded = true;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _loaded = true;
        await LoadAsync(cancellationToken);
        ProfileUpdated?.Invoke();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var profile = await profileApi.GetMyProfileAsync(cancellationToken);
        DisplayName = profile?.DisplayName;
    }
}
