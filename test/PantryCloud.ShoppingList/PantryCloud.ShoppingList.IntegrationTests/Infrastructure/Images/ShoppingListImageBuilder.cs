using DotNet.Testcontainers.Builders;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;
using PantryCloud.ShoppingList.IntegrationTests.Constants;

namespace PantryCloud.ShoppingList.IntegrationTests.Infrastructure.Images;

public static class ShoppingListImageBuilder
{
    public const string ImageName = "pantrycloud-shoppinglist-test:latest";

    public static async Task BuildAsync(CancellationToken ct = default)
    {
        var solutionDir = CommonDirectoryPath.GetSolutionDirectory().DirectoryPath;
        await DockerImageHelper.BuildImageAsync(ImageName, TestConstants.DockerFilePath, solutionDir, ct, forceRebuild: true);
    }
}
