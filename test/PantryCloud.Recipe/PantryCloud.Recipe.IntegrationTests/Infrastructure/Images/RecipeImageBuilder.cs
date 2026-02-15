using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using PantryCloud.Recipe.IntegrationTests.Constants;

namespace PantryCloud.Recipe.IntegrationTests.Infrastructure.Images;

public static class RecipeImageBuilder
{
    public static IFutureDockerImage Build()
    {
        return new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile(TestConstants.DockerFilePath)
            .Build();
    }
}
