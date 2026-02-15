using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.Notification.Presentation.Controllers;
using Shouldly;

namespace PantryCloud.Notification.UnitTests.Controllers;

public class NotificationsControllerTests
{
    [Fact]
    public async Task GetMyNotifications_ReturnsOkWithList_WhenUserAuthenticated()
    {
        var userId = Guid.NewGuid();
        var expected = new List<NotificationDto>
        {
            new(Guid.NewGuid(), "Test Title", "Test Message", NotificationType.Info, DateTime.UtcNow.AddMinutes(-1), userId, null)
        };
        var repository = Substitute.For<IUserNotificationRepository>();
        repository.GetByUserIdAsync(userId, 50, null, Arg.Any<CancellationToken>()).Returns(expected);

        var controller = new NotificationsController(repository)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            HttpContext =
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"))
            }
        };

        var result = await controller.GetMyNotifications();

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var list = okResult.Value.ShouldBeOfType<List<NotificationDto>>();
        list.Count.ShouldBe(1);
        list[0].Title.ShouldBe("Test Title");
        list[0].Message.ShouldBe("Test Message");
        await repository.Received(1).GetByUserIdAsync(userId, 50, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyNotifications_ReturnsUnauthorized_WhenUserHasNoSubClaim()
    {
        var repository = Substitute.For<IUserNotificationRepository>();
        var controller = new NotificationsController(repository)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            HttpContext =
            {
                User = new ClaimsPrincipal(new ClaimsIdentity()) // No name
            }
        };

        var result = await controller.GetMyNotifications();

        result.ShouldBeOfType<UnauthorizedResult>();
        await repository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyNotifications_ClampsLimitTo100_WhenLimitOver100()
    {
        var userId = Guid.NewGuid();
        var repository = Substitute.For<IUserNotificationRepository>();
        repository.GetByUserIdAsync(userId, 100, null, Arg.Any<CancellationToken>()).Returns([]);

        var controller = new NotificationsController(repository)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            HttpContext =
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"))
            }
        };

        await controller.GetMyNotifications(200);

        await repository.Received(1).GetByUserIdAsync(userId, 100, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyNotifications_PassesSinceToRepository_WhenProvided()
    {
        var userId = Guid.NewGuid();
        var since = DateTime.UtcNow.AddHours(-2);
        var repository = Substitute.For<IUserNotificationRepository>();
        repository.GetByUserIdAsync(userId, 50, since, Arg.Any<CancellationToken>()).Returns([]);

        var controller = new NotificationsController(repository)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            HttpContext =
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"))
            }
        };

        await controller.GetMyNotifications(50, since);

        await repository.Received(1).GetByUserIdAsync(userId, 50, since, Arg.Any<CancellationToken>());
    }
}
