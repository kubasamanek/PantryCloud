using MediatR;
using NetArchTest.Rules;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Entities;
using PantryCloud.Notification.Presentation.Controllers;
using PantryCloud.SharedKernel.Controllers;
using PantryCloud.SharedKernel.Entities;
using Shouldly;
using Xunit;

namespace PantryCloud.Notification.UnitTests.Architecture;

public class DesignTests
{
    private const string CoreNamespace = "PantryCloud.Notification.Core";

    [Fact]
    public void Domain_Entities_Should_Reside_In_Core_Layer()
    {
        var assembly = typeof(UserNotification).Assembly;

        var result = Types.InAssembly(assembly)
            .That()
            .Inherit(typeof(BaseEntity))
            .Or()
            .Inherit(typeof(AuditableEntity))
            .Should()
            .ResideInNamespace(CoreNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Command_And_Query_Requests_Should_End_With_Command_Or_Query()
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        var result = Types.InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(IRequest<>))
            .Or()
            .ImplementInterface(typeof(IRequest))
            .Should()
            .HaveNameEndingWith("Command")
            .Or()
            .HaveNameEndingWith("Query")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Handlers_Should_End_With_Handler()
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        var result = Types.InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Controllers_Should_Inherit_From_ApiControllerBase()
    {
        var assembly = typeof(NotificationsController).Assembly;

        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .Inherit(typeof(ApiControllerBase))
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
