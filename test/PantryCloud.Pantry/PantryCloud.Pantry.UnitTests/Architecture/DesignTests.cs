using MediatR;
using NetArchTest.Rules;
using PantryCloud.SharedKernel.Entities;
using PantryCloud.SharedKernel.Controllers;
using Shouldly;
using Xunit;

namespace PantryCloud.Pantry.UnitTests.Architecture;

public class DesignTests
{
    private const string CoreNamespace = "PantryCloud.Pantry.Core";

    [Fact]
    public void Domain_Entities_Should_Reside_In_Core_Layer()
    {
        var assembly = typeof(Core.Entities.PantryItem).Assembly;

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
        var assembly = typeof(Application.ServiceCollectionExtensions).Assembly;

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
        var assembly = typeof(Application.ServiceCollectionExtensions).Assembly;

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
        var assembly = typeof(Presentation.Controllers.PantryController).Assembly;

        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .Inherit(typeof(ApiControllerBase))
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
