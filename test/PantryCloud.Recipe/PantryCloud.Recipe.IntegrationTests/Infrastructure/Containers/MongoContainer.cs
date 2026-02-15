using DotNet.Testcontainers.Networks;
using PantryCloud.Recipe.IntegrationTests.Infrastructure;
using PantryCloud.SharedKernel.Testing.Infrastructure.TestContainers;
using Testcontainers.MongoDb;

namespace PantryCloud.Recipe.IntegrationTests.Infrastructure.Containers;

public static class MongoContainer
{
    public static MongoDbContainer Create(INetwork network)
    {
        return new MongoDbBuilder()
            .WithNetwork(network)
            .WithNetworkAliases(IntegrationConstants.Mongo.Host)
            .WithCreateParameterModifier(ContainerLoggingConfig.JsonFileLogging)
            .Build();
    }
}
