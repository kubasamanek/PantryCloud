using DotNet.Testcontainers.Builders;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;

namespace PantryCloud.Identity.IntegrationTests.Infrastructure.Images;

public static class IdentityImageBuilder
{
    public const string ImageName = "pantrycloud-identity-test:latest";
    private const string DockerfilePath = "src/Services/PantryCloud.Identity/PantryCloud.Identity.Presentation/Dockerfile";

    public static async Task BuildAsync(CancellationToken ct = default)
    {
        var solutionDir = CommonDirectoryPath.GetSolutionDirectory().DirectoryPath;
        await DockerImageHelper.BuildImageAsync(ImageName, DockerfilePath, solutionDir, ct);
    }
}
