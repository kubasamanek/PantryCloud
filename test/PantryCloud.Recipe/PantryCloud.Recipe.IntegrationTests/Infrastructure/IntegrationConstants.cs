namespace PantryCloud.Recipe.IntegrationTests.Infrastructure;

public static class IntegrationConstants
{
    public static class Mongo
    {
        public const string Host = "mongo";
        public const int Port = 27017;
        public const string DatabaseName = "recipe_db";
    }

    public static class Recipe
    {
        public const ushort Port = 8080;
        public const string HttpPorts = "8080";
        public const string NetworkAlias = "recipe-api";
    }

    public static class Environment
    {
        public const string Development = "Development";
    }
}
