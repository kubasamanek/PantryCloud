using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using PantryCloud.ShoppingList.IntegrationTests.Constants;

namespace PantryCloud.ShoppingList.IntegrationTests.Infrastructure.Images;

public static class ShoppingListImageBuilder
{
    public static IFutureDockerImage Build()
    {
        return new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile(TestConstants.DockerFilePath)
            .Build();
    }
}
