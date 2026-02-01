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
}
