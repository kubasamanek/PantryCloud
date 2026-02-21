using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using PantryCloud.Household.IntegrationTests.Constants;

namespace PantryCloud.Household.IntegrationTests.Infrastructure.Images;

public static class HouseholdImageBuilder
{
    public static IFutureDockerImage Build()
    {
        return new ImageFromDockerfileBuilder()
            .WithName("pantrycloud-household-test")
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile(TestConstants.DockerFilePath)
            .WithDeleteIfExists(false)
            .Build();
    }
}