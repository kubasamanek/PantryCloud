using DotNet.Testcontainers.Builders;
using PantryCloud.Household.IntegrationTests.Constants;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

namespace PantryCloud.Household.IntegrationTests.Infrastructure.Images;

public static class HouseholdImageBuilder
{
    public const string ImageName = "pantrycloud-household-test:latest";

    public static async Task BuildAsync(CancellationToken ct = default)
    {
        var solutionDir = CommonDirectoryPath.GetSolutionDirectory().DirectoryPath;
        await DockerImageHelper.BuildImageAsync(ImageName, TestConstants.DockerFilePath, solutionDir, ct, forceRebuild: true);
    }
}
