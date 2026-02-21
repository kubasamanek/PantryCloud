using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;

namespace PantryCloud.Notification.IntegrationTests.Infrastructure.Images;

public static class NotificationImageBuilder
{
    public static IFutureDockerImage Build()
    {
        return new ImageFromDockerfileBuilder()
            .WithName("pantrycloud-notification-test")
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile("src/Services/PantryCloud.Notification/PantryCloud.Notification.Presentation/Dockerfile")
            .WithDeleteIfExists(false)
            .Build();
    }
}
