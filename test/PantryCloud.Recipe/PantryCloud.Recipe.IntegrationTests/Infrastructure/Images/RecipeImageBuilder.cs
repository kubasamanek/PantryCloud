using DotNet.Testcontainers.Builders;
using PantryCloud.Recipe.IntegrationTests.Constants;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

namespace PantryCloud.Recipe.IntegrationTests.Infrastructure.Images;

public static class RecipeImageBuilder
{
    public const string ImageName = "pantrycloud-recipe-test:latest";

    public static async Task BuildAsync(CancellationToken ct = default)
    {
        var solutionDir = CommonDirectoryPath.GetSolutionDirectory().DirectoryPath;
        await DockerImageHelper.BuildImageAsync(ImageName, TestConstants.DockerFilePath, solutionDir, ct);
    }
}
