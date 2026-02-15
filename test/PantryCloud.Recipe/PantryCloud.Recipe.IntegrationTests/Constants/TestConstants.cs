namespace PantryCloud.Recipe.IntegrationTests.Constants;

public static class TestConstants
{
    public const string DockerFilePath =
        "src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Presentation/Dockerfile";

    public static class Endpoints
    {
        private const string Base = "/api/recipes";
        public const string Search = Base + "/search";
        public const string Recommend = Base + "/recommend";
        public const string Seed = Base + "/seed";
        public static string GetRecipe(Guid id) => Base + $"/{id}";
    }

    public static class Jwt
    {
        public const string Issuer = "pantry-identity";
        public const string Audience = "pantry-cloud";
    }
}
