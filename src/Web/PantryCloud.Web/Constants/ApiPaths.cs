namespace PantryCloud.Web.Constants;

/// <summary>
/// Gateway API base paths. Used by services that call the gateway.
/// All paths include the API version segment (v1) to match gateway routing.
/// </summary>
public static class ApiPaths
{
    private const string Version = "v1";

    public const string Households = "api/" + Version + "/household/households";

    public const string Pantry = "api/" + Version + "/pantry/pantry";

    public const string ShoppingList = "api/" + Version + "/shoppinglist/shopping-lists";

    public const string Recipe = "api/" + Version + "/recipe/recipes";

    public const string Notification = "api/" + Version + "/notification/notifications";
}
