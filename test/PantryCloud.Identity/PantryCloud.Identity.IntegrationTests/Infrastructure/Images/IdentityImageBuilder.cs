using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;

namespace PantryCloud.Identity.IntegrationTests.Infrastructure.Images;

public static class IdentityImageBuilder
{
    public static IFutureDockerImage Build()
    {
        return new ImageFromDockerfileBuilder()
            .WithName("pantrycloud-identity-test")
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile("src/Services/PantryCloud.Identity/PantryCloud.Identity.Presentation/Dockerfile")
            .WithDeleteIfExists(false)
            .Build();
    }
}
