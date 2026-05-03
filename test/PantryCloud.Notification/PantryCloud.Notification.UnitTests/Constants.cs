namespace PantryCloud.Notification.UnitTests;

internal static class Constants
{
    public static class User
    {
        public static readonly Guid Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public const string Email = "test@example.com";
        public const string OrphanEmail = "orphan@example.com";
        public const string NewOwnerEmail = "newowner@example.com";
    }

    public static class Household
    {
        public static readonly Guid Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public static readonly Guid OldId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    }

    public static class NotificationTitles
    {
        public const string WelcomeToHousehold = "Welcome to the Household";
        public const string MemberJoinedHousehold = "Member Joined Household";
        public const string MemberLeftHousehold = "Member Left Household";
        public const string OwnershipTransferred = "Ownership Transferred";
        public const string NewShoppingList = "New Shopping List";
        public const string ShoppingListComplete = "Shopping List Complete";
        public const string PantryItemDepleted = "Pantry Item Depleted";
        public const string PantryItemsExpiringSoon = "Pantry Items Expiring Soon";
        public const string PreferencesUpdated = "Preferences Updated";
    }

    public static class TestData
    {
        public const string ShoppingListName = "Weekly Groceries";
        public const string ItemNameMilk = "Milk";
        public const string ItemNameBread = "Bread";
        public const string ItemNameEggs = "Eggs";
        public const string HouseholdOwnerMessage = "household owner";
        public const string DietaryPreferences = "dietary preferences";
        public const string EatenCompletely = "eaten completely";
        public const string CheckedOff = "checked off";
        public const string Created = "created";
        public const string ItemsExpiringSoon = "Items expiring soon";
    }

    public static class Dates
    {
        public static readonly DateTime ThirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        public static readonly DateTime TenDaysAgo = DateTime.UtcNow.AddDays(-10);
        public static readonly DateTime OneDayAgo = DateTime.UtcNow.AddDays(-1);
        public static readonly DateTime OneHourAgo = DateTime.UtcNow.AddHours(-1);
    }
}
