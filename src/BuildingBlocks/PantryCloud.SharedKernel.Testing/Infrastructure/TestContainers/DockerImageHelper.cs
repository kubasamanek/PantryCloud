using System.Diagnostics;

namespace PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

/// <summary>
/// Builds Docker images via the CLI (which uses BuildKit) instead of the Docker Engine API.
/// Testcontainers-dotnet's ImageFromDockerfileBuilder uses the legacy builder via the API,
/// which has a known bug where intermediate layers in multistage Dockerfiles randomly disappear.
/// See https://github.com/testcontainers/testcontainers-dotnet/issues/914
/// </summary>
public static class DockerImageHelper
{
    /// <summary>
    /// Builds a Docker image from a Dockerfile using the Docker CLI.
    /// Skips the build if the image already exists locally.
    /// </summary>
    /// <param name="imageName">Tag for the built image (e.g. "pantrycloud-household-test:latest")</param>
    /// <param name="dockerfilePath">Relative path to the Dockerfile from the build context</param>
    /// <param name="buildContext">Absolute path to the build context directory</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task BuildImageAsync(
        string imageName,
        string dockerfilePath,
        string buildContext,
        CancellationToken cancellationToken = default)
    {
        if (await ImageExistsAsync(imageName, cancellationToken))
            return;

        var psi = new ProcessStartInfo("docker", $"build -t {imageName} -f {dockerfilePath} .")
        {
            WorkingDirectory = buildContext,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start docker build process.");

        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"docker build failed (exit code {process.ExitCode}) for image '{imageName}'.\n{stderr}");
    }

    private static async Task<bool> ImageExistsAsync(string imageName, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo("docker", $"image inspect {imageName}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
            return false;

        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode == 0;
    }
}
