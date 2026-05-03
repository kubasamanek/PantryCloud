namespace PantryCloud.ShoppingList.IntegrationTests.Infrastructure;

public static class IntegrationConstants
{
    public static class Postgres
    {
        public const string Host = "postgres";
        public const string User = "admin";
        public const string Password = "password";
        public const string DefaultDatabase = "postgres";
        public const string ShoppingListDatabase = "shoppinglist_db";
        public const int Port = 5432;
    }

    public static class ShoppingList
    {
        public const ushort Port = 8080;
        public const string HttpPorts = "8080";
        public const string NetworkAlias = "shoppinglist-api";
    }

    public static class Environment
    {
        public const string Testing = "Testing";
    }
}
