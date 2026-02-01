using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;

namespace PantryCloud.Identity.IntegrationTests.Infrastructure.Images;

public static class IdentityImageBuilder
{
    public static IFutureDockerImage Build()
    {
        return new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile("src/Services/PantryCloud.Identity/PantryCloud.Identity.Presentation/Dockerfile")
            .Build();
    }
}
