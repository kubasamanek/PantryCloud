using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using PantryCloud.Audit.IntegrationTests.Constants;

namespace PantryCloud.Audit.IntegrationTests.Infrastructure.Images;

public static class AuditImageBuilder
{
    public static IFutureDockerImage Build()
    {
        return new ImageFromDockerfileBuilder()
            .WithName("pantrycloud-audit-test")
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile(TestConstants.DockerFilePath)
            .WithDeleteIfExists(false)
            .Build();
    }
}
