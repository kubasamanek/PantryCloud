using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using PantryCloud.Web.Services.Auth;

namespace PantryCloud.Web.Services.Notification;

public class NotificationHubClient(
    ITokenStorage tokenStorage,
    IOptions<GatewayOptions> gatewayOptions) : IAsyncDisposable, INotificationHubClient
{
    private HubConnection? _connection;
    private readonly Lock _lock = new();

    public event Action<NotificationMessage>? OnNotification;
    public event Action<HubConnectionState>? OnStateChanged;

    public HubConnectionState State => _connection?.State ?? HubConnectionState.Disconnected;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_connection != null)
        {
            if (_connection.State is HubConnectionState.Connected or HubConnectionState.Connecting)
            {
                return;
            }

            await _connection.DisposeAsync();
            _connection = null;
        }

        var baseUrl = (gatewayOptions.Value?.BaseUrl ?? "http://localhost:5050").TrimEnd('/');
        var hubUrl = $"{baseUrl}/api/notification/hubs/notifications";

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = async () => await tokenStorage.GetAccessTokenAsync(cancellationToken) ?? "";
            })
            .WithAutomaticReconnect()
            .AddJsonProtocol()
            .Build();

        connection.On<NotificationMessage>("ReceiveNotification", msg =>
        {
            OnNotification?.Invoke(msg);
        });

        connection.Reconnecting += _ =>
        {
            OnStateChanged?.Invoke(HubConnectionState.Reconnecting);
            return Task.CompletedTask;
        };
        connection.Reconnected += _ =>
        {
            OnStateChanged?.Invoke(HubConnectionState.Connected);
            return Task.CompletedTask;
        };
        connection.Closed += _ =>
        {
            OnStateChanged?.Invoke(HubConnectionState.Disconnected);
            return Task.CompletedTask;
        };

        lock (_lock)
            _connection = connection;

        await connection.StartAsync(cancellationToken);
        OnStateChanged?.Invoke(HubConnectionState.Connected);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        var connection = _connection;
        if (connection == null) return;
        lock (_lock)
            _connection = null;
        await connection.StopAsync(cancellationToken);
        await connection.DisposeAsync();
        OnStateChanged?.Invoke(HubConnectionState.Disconnected);
    }

    public async ValueTask DisposeAsync() => await StopAsync();
}
