namespace PantryCloud.Notification.IntegrationTests;

public static class Constants
{
    public static class Postgres
    {
        public const string Host = "postgres";
        public const string User = "admin";
        public const string Password = "password";
        public const string DefaultDatabase = "postgres";
        public const string NotificationDatabase = "notification_db";
        public const int Port = 5432;
    }

    public static class RabbitMq
    {
        public const string Host = "rabbitmq";
        public const string User = "admin";
        public const string Password = "password";
        public const int AmqpPort = 5672;
    }

    public static class Notification
    {
        public const ushort Port = 8080;
        public const string HttpPorts = "8080";
        public const string NetworkAlias = "notification-api";
        public const string HubPath = "/hubs/notifications";
    }

    public static class Jwt
    {
        public const string Issuer = "pantry-identity";
        public const string Audience = "pantry-cloud";
    }

    public static class Environment
    {
        public const string Testing = "Testing";
    }

    public static class TestData
    {
        public const string UserAEmail = "usera@test.com";
        public const string UserBEmail = "userb@test.com";
        public const string NewOwnerEmail = "newowner@test.com";
        public const string ShoppingListName = "Weekly Groceries";
        public const string ItemNameMilk = "Milk";
        public const string ItemNameBread = "Bread";
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

    public static class Delays
    {
        public const int ShortMs = 1500;
        public const int DefaultMs = 2000;
    }
}
