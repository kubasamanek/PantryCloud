using DotNet.Testcontainers.Networks;
using PantryCloud.Recipe.IntegrationTests.Infrastructure;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;
using Testcontainers.MongoDb;

namespace PantryCloud.Recipe.IntegrationTests.Infrastructure.Containers;

public static class MongoContainer
{
    /// <summary>
    /// Creates a MongoDB container with no auth so the Recipe API can connect with mongodb://mongo:27017/
    /// </summary>
    public static MongoDbContainer Create(INetwork network)
    {
        return new MongoDbBuilder()
            .WithUsername(string.Empty)
            .WithPassword(string.Empty)
            .WithNetwork(network)
            .WithNetworkAliases(IntegrationConstants.Mongo.Host)
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging)
            .Build();
    }
}
