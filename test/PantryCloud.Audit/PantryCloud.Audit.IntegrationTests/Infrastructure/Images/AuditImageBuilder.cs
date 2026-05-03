using DotNet.Testcontainers.Builders;
using PantryCloud.Audit.IntegrationTests.Constants;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

namespace PantryCloud.Audit.IntegrationTests.Infrastructure.Images;

public static class AuditImageBuilder
{
    public const string ImageName = "pantrycloud-audit-test:latest";

    public static async Task BuildAsync(CancellationToken ct = default)
    {
        var solutionDir = CommonDirectoryPath.GetSolutionDirectory().DirectoryPath;
        await DockerImageHelper.BuildImageAsync(ImageName, TestConstants.DockerFilePath, solutionDir, ct, forceRebuild: true);
    }
}
