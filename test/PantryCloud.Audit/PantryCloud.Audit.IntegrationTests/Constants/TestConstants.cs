namespace PantryCloud.Audit.IntegrationTests.Constants;

public static class TestConstants
{
    public const string DockerFilePath =
        "src/Services/PantryCloud.Audit/PantryCloud.Audit.Presentation/Dockerfile";

    public static class Endpoints
    {
        private const string Base = "/api/audit";
        public static string ListHouseholdEntries(Guid householdId) => Base + $"/households/{householdId}/entries";
    }

    public static class Jwt
    {
        public const string Issuer = "pantry-identity";
        public const string Audience = "pantry-cloud";
    }
}
