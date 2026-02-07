namespace PantryCloud.Recipe.UnitTests;

internal static class Constants
{
    public static class Recipe
    {
        public static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public const string TestTitle = "Test Recipe";
    }

    public static class Search
    {
        public static readonly IReadOnlyList<string> IngredientsChickenRice = ["chicken", "rice"];
        public static readonly IReadOnlyList<string> IngredientsGarlic = ["garlic"];
        public static readonly IReadOnlyList<string> IngredientsChickenEmpty = ["chicken", ""];
    }

    public static class Recommend
    {
        public const int Limit10 = 10;
        public const int Limit5 = 5;
        public const int Limit50 = 50;
        public const int Limit0 = 0;
        public const int Limit101 = 101;
        public static readonly IReadOnlyList<string> IngredientHintsChicken = ["chicken"];
    }

    public static class Preferences
    {
        public const string Vegetarian = "Vegetarian";
        public const string Pescatarian = "Pescatarian";
        public const string Invalid = "Invalid";
        public static readonly IReadOnlyList<string> Nuts = ["nuts"];
        public static readonly IReadOnlyList<string> NutsShellfish = ["nuts", "shellfish"];
    }
    
    public static class Errors
    {
        public const string RecipeNotFound = "Recipe.NotFound";
        public const string InvalidSearchRequest = "Recipe.InvalidSearchRequest";
    }
}
