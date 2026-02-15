namespace PantryCloud.Pantry.IntegrationTests.Constants;

public static class TestConstants
{
    public const string DockerFilePath =
        "src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Presentation/Dockerfile";

    public static class Endpoints
    {
        private const string Base = "/api/pantry";
        public const string CreateItem = Base + "/items";
        public const string ListItems = Base + "/items";
        public static string GetItem(Guid id) => Base + $"/items/{id}";
        public static string UpdateItem(Guid id) => Base + $"/items/{id}";
        public static string DeleteItem(Guid id) => Base + $"/items/{id}";
    }

    public static class Jwt
    {
        public const string Issuer = "pantry-identity";
        public const string Audience = "pantry-cloud";
    }

    public static class TestData
    {
        public const string ItemName = "Test Milk";
        public const string Category = "Dairy";
    }
}
