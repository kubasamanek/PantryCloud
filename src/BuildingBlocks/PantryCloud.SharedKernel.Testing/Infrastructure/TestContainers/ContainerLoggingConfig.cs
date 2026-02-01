using Docker.DotNet.Models;

namespace PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

/// <summary>
/// Shared TestContainers configuration. Use json-file logging so TestContainers can read container logs.
/// </summary>
public static class ContainerLoggingConfig
{
    public static Action<CreateContainerParameters> JsonFileLogging => p =>
    {
        p.HostConfig ??= new HostConfig();
        p.HostConfig.LogConfig = new LogConfig { Type = "json-file" };
    };
}
