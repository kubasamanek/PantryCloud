using NetArchTest.Rules;
using PantryCloud.ShoppingList.Core.Entities;
using PantryCloud.ShoppingList.Presentation.Controllers;
using Shouldly;
using Xunit;

namespace PantryCloud.ShoppingList.UnitTests.Architecture;

public class LayerTests
{
    private const string InfrastructureNamespace = "PantryCloud.ShoppingList.Infrastructure";
    private const string PresentationNamespace = "PantryCloud.ShoppingList.Presentation";
    private const string ApplicationNamespace = "PantryCloud.ShoppingList.Application";

    [Fact]
    public void Core_Should_Not_Have_Dependency_On_Other_Layers()
    {
        var assembly = typeof(PantryCloud.ShoppingList.Core.Entities.ShoppingList).Assembly;

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
