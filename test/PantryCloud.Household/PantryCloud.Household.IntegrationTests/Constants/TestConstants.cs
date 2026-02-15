namespace PantryCloud.Household.IntegrationTests.Constants;

public static class TestConstants
{
    public const string DockerFilePath =
        "src/Services/PantryCloud.Household/PantryCloud.Household.Presentation/Dockerfile";
    
    public static class Endpoints
    {
        private const string Base = "/api/households";
        public const string CreateHousehold = Base + "";
        public const string GetCurrentHousehold = Base + "/me";
        public const string SendInvite = Base + "/invite";
        public const string Join = Base + "/join";
        public const string Leave = Base + "/leave";
        public const string KickMember = Base + "/members";
        public const string TransferOwnership = Base + "/transfer-ownership";
        public const string GetMyPreferences = Base + "/me/preferences";
        public const string UpdateMyPreferences = Base + "/me/preferences";
        public const string GetMyProfile = Base + "/me/profile";
        public const string UpdateMyProfile = Base + "/me/profile";
        public const string GetHouseholdMembersPreferences = Base + "/me/members/preferences";
    }

    public static class Jwt
    {
        public const string Issuer = "pantry-identity";
        public const string Audience = "pantry-cloud";
    }

    public static class TestData
    {
        public const string HouseholdName = "Test Household";
        public const string InviteeEmail = "invitee@test.com";
    }
}
