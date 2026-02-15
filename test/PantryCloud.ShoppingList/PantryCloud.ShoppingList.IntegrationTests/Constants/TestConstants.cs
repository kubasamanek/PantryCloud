namespace PantryCloud.ShoppingList.IntegrationTests.Constants;

public static class TestConstants
{
    public const string DockerFilePath =
        "src/Services/PantryCloud.ShoppingList/PantryCloud.ShoppingList.Presentation/Dockerfile";

    public static class Endpoints
    {
        private const string Base = "/api/shopping-lists";
        public const string CreateList = Base + "";
        public const string ListLists = Base + "";
        public static string GetList(Guid id) => Base + $"/{id}";
        public static string DeleteList(Guid id) => Base + $"/{id}";
        public static string AddItem(Guid listId) => Base + $"/{listId}/items";
        public static string AddItemsBatch(Guid listId) => Base + $"/{listId}/items/batch";
        public static string UpdateItem(Guid listId, Guid itemId) => Base + $"/{listId}/items/{itemId}";
        public static string DeleteItem(Guid listId, Guid itemId) => Base + $"/{listId}/items/{itemId}";
        public static string CheckItem(Guid listId, Guid itemId) => Base + $"/{listId}/items/{itemId}/check";
    }

    public static class Jwt
    {
        public const string Issuer = "pantry-identity";
        public const string Audience = "pantry-cloud";
    }

    public static class TestData
    {
        public const string ListName = "Weekly groceries";
        public const string AnotherListName = "Party list";
        public const string ItemName = "Milk";
        public const string ItemName2 = "Bread";
    }
}
