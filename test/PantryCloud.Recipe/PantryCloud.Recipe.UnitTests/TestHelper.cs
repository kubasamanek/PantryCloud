using Microsoft.Extensions.Logging;
using NSubstitute;

namespace PantryCloud.Recipe.UnitTests;

internal static class TestHelper
{
    public static ILogger<T> MockLogger<T>() where T : class
        => Substitute.For<ILogger<T>>();
}
