namespace PantryCloud.Web.Constants;

/// <summary>
/// Gateway API base paths. Used by services that call the gateway.
/// </summary>
public static class ApiPaths
{
    /// <summary>
    /// Base path for household and profile endpoints (household service).
    /// </summary>
    public const string Households = "api/household/api/households";

    /// <summary>
    /// Base path for pantry endpoints (pantry service).
    /// </summary>
    public const string Pantry = "api/pantry";

    /// <summary>
    /// Base path for shopping list endpoints (shopping list service).
    /// </summary>
    public const string ShoppingList = "api/shoppinglist";

    /// <summary>
    /// Base path for recipe endpoints (recipe service).
    /// </summary>
    public const string Recipe = "api/recipe";
}
