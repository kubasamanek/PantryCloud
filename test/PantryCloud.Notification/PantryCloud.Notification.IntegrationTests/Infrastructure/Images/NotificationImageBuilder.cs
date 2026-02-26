using DotNet.Testcontainers.Builders;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

namespace PantryCloud.Notification.IntegrationTests.Infrastructure.Images;

public static class NotificationImageBuilder
{
    public const string ImageName = "pantrycloud-notification-test:latest";
    private const string DockerfilePath = "src/Services/PantryCloud.Notification/PantryCloud.Notification.Presentation/Dockerfile";

    public static async Task BuildAsync(CancellationToken ct = default)
    {
        var solutionDir = CommonDirectoryPath.GetSolutionDirectory().DirectoryPath;
        await DockerImageHelper.BuildImageAsync(ImageName, DockerfilePath, solutionDir, ct, forceRebuild: true);
    }
}
