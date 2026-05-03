namespace PantryCloud.Pantry.IntegrationTests.Infrastructure;

public static class IntegrationConstants
{
    public static class Postgres
    {
        public const string Host = "postgres";
        public const string User = "admin";
        public const string Password = "password";
        public const string DefaultDatabase = "postgres";
        public const string PantryDatabase = "pantry_db";
        public const int Port = 5432;
    }

    public static class Pantry
    {
        public const ushort Port = 8080;
        public const string HttpPorts = "8080";
        public const string NetworkAlias = "pantry-api";
    }

    public static class Environment
    {
        public const string Testing = "Testing";
    }
}
