using System.Security.Claims;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PantryCloud.Notification.Application.Queries;
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
        var expectedNotifications = new List<NotificationDto>
        {
            new(Guid.NewGuid(), "Test Title", "Test Message", NotificationType.Info, DateTime.UtcNow.AddMinutes(-1), userId, null)
        };
        
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();

        mediator.Send(Arg.Any<GetMyNotificationsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedNotifications);

        var controller = new NotificationsController(mediator, mapper)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            HttpContext =
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"))
            }
        };

        var result = await controller.GetMyNotifications();

        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        var list = objectResult.Value.ShouldBeOfType<List<NotificationDto>>();
        list.Count.ShouldBe(1);
        list[0].Title.ShouldBe("Test Title");
        
        await mediator.Received(1).Send(
            Arg.Is<GetMyNotificationsQuery>(q => q.UserId == userId && q.Limit == 50 && q.Since == null), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyNotifications_ReturnsUnauthorized_WhenUserHasNoSubClaim()
    {
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();
        
        var controller = new NotificationsController(mediator, mapper)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            HttpContext =
            {
                User = new ClaimsPrincipal(new ClaimsIdentity()) // No name
            }
        };

        var result = await controller.GetMyNotifications();

        result.ShouldBeOfType<UnauthorizedResult>();
        await mediator.DidNotReceive().Send(Arg.Any<GetMyNotificationsQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyNotifications_ClampsLimitTo100_WhenLimitOver100()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();

        mediator.Send(Arg.Any<GetMyNotificationsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<NotificationDto>());

        var controller = new NotificationsController(mediator, mapper)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            HttpContext =
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"))
            }
        };

        await controller.GetMyNotifications(200);

        await mediator.Received(1).Send(
            Arg.Is<GetMyNotificationsQuery>(q => q.UserId == userId && q.Limit == 100), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyNotifications_PassesSinceToRepository_WhenProvided()
    {
        var userId = Guid.NewGuid();
        var since = DateTime.UtcNow.AddHours(-2);
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();

        mediator.Send(Arg.Any<GetMyNotificationsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<NotificationDto>());

        var controller = new NotificationsController(mediator, mapper)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            HttpContext =
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"))
            }
        };

        await controller.GetMyNotifications(50, since);

        await mediator.Received(1).Send(
            Arg.Is<GetMyNotificationsQuery>(q => q.UserId == userId && q.Since == since), 
            Arg.Any<CancellationToken>());
    }
}
