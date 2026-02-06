namespace PantryCloud.Audit.UnitTests;

internal static class Constants
{
    public static class Audit
    {
        public static readonly Guid HouseholdId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public static readonly Guid ItemId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        public static readonly Guid EventId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        public const string ItemName = "Milk";
        public const decimal Quantity = 2.5m;
        public const string CorrelationId = "test-correlation";
        public const string ActionCreated = "Created";
        public const string ActionUpdated = "Updated";
        public const string ActionDeleted = "Deleted";
        public const string ActionDepleted = "Depleted";
        public const string ActionJoined = "Joined";
        public const string ActionLeft = "Left";
        public const string ActionOwnershipTransferred = "OwnershipTransferred";
        public const string ActionPreferenceAdded = "PreferenceAdded";
        public const string ActionAllItemsChecked = "AllItemsChecked";
        public const string EntityPantryItem = "PantryItem";
        public const string EntityMember = "Member";
        public const string EntityHousehold = "Household";
        public const string EntityPreference = "Preference";
        public const string EntityShoppingList = "ShoppingList";
        public const string ShoppingListName = "Weekly groceries";
        public const string MemberEmail = "member@example.com";
    }
}
