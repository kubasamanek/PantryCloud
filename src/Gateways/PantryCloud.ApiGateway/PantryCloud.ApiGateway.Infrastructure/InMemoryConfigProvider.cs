using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace PantryCloud.ApiGateway.Infrastructure;

/// <summary>
/// Provides an in-memory storage mechanism for YARP configuration (Routes and Clusters).
/// Implements the IProxyConfigProvider to allow for dynamic, zero-downtime updates 
/// to the reverse proxy configuration without restarting the application.
/// </summary>
public class InMemoryConfigProvider(RouteConfig[] routes, ClusterConfig[] clusters) : IProxyConfigProvider
{
    private volatile InMemoryConfig _config = new(routes, clusters);

    public IProxyConfig GetConfig() => _config;

    public void Update(RouteConfig[] routes, ClusterConfig[] clusters)
    {
        var oldConfig = _config;
        _config = new InMemoryConfig(routes, clusters);
        oldConfig.SignalChange();
    }
}

public class InMemoryConfig : IProxyConfig
{
    private readonly CancellationTokenSource _cts = new();

    public InMemoryConfig(RouteConfig[] routes, ClusterConfig[] clusters)
    {
        Routes = routes;
        Clusters = clusters;
        ChangeToken = new CancellationChangeToken(_cts.Token);
    }

    public IReadOnlyList<RouteConfig> Routes { get; }
    public IReadOnlyList<ClusterConfig> Clusters { get; }
    public IChangeToken ChangeToken { get; }

    internal void SignalChange()
    {
        _cts.Cancel();
    }
}

