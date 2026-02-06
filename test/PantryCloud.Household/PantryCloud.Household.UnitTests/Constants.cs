namespace PantryCloud.Household.UnitTests;

internal static class Constants
{
    public static class User
    {
        public static readonly Guid Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public const string Email = "user@example.com";
    }

    public static class Household
    {
        public static readonly Guid Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public const string Name = "Test Household";
    }

    public static class Invitation
    {
        public const string InviteeEmail = "invitee@example.com";
        public const string ValidCode = "VALID_CODE";
    }

    public static class Profile
    {
        public const string DisplayName = "Test User";
        public const string NewDisplayName = "My Display Name";
        public const string OldDisplayName = "Old Name";
        public const string UpdatedDisplayName = "New Name";
        public const string AvatarUrl = "https://example.com/avatar.png";
        public const string OldAvatarUrl = "https://old.com/avatar.png";
    }

    public static class MemberIds
    {
        public static readonly Guid MemberToKick = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        public static readonly Guid NewOwner = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        public static readonly Guid NonMember = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    }

    public static class HouseholdNames
    {
        public const string MyHousehold = "My Household";
        public const string AnotherOne = "Another One";
        public const string SoloHouse = "Solo House";
        public const string TeamHouse = "Team House";
        public const string SharedHouse = "Shared House";
        public const string Target = "Target";
    }

    public static class Errors
    {
        public const string NotFound = "Household.NotFound";
        public const string MemberNotFound = "Household.Member.NotFound";
    }
}