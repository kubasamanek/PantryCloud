using DotNet.Testcontainers.Builders;
using PantryCloud.Pantry.IntegrationTests.Constants;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

namespace PantryCloud.Pantry.IntegrationTests.Infrastructure.Images;

public static class PantryImageBuilder
{
    public const string ImageName = "pantrycloud-pantry-test:latest";

    public static async Task BuildAsync(CancellationToken ct = default)
    {
        var solutionDir = CommonDirectoryPath.GetSolutionDirectory().DirectoryPath;
        await DockerImageHelper.BuildImageAsync(ImageName, TestConstants.DockerFilePath, solutionDir, ct, forceRebuild: true);
    }
}
