using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace PantryCloud.Pantry.UnitTests.Architecture;

public class LayerTests
{
    private const string ApplicationNamespace = "PantryCloud.Pantry.Application";
    private const string InfrastructureNamespace = "PantryCloud.Pantry.Infrastructure";
    private const string PresentationNamespace = "PantryCloud.Pantry.Presentation";

    [Fact]
    public void Core_Should_Not_Have_Dependency_On_Other_Layers()
    {
        var assembly = typeof(Core.Entities.PantryItem).Assembly;

        var otherLayers = new[]
        {
            ApplicationNamespace,
            InfrastructureNamespace,
            PresentationNamespace
        };

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAll(otherLayers)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Application_Should_Not_Have_Dependency_On_Infrastructure_Or_Presentation()
    {
        var assembly = typeof(Application.ServiceCollectionExtensions).Assembly;

        var otherLayers = new[]
        {
            InfrastructureNamespace,
            PresentationNamespace
        };

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAll(otherLayers)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Infrastructure_Should_Not_Have_Dependency_On_Presentation()
    {
        var assembly = typeof(Infrastructure.ServiceCollectionExtensions).Assembly;

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(PresentationNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }


}
