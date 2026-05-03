namespace PantryCloud.Web.Helpers;

/// <summary>
/// Display labels for Pantry Unit (backend enum serialized as int).
/// </summary>
public static class UnitDisplayHelper
{
    public static string UnitLabel(int unit) => unit switch
    {
        0 => "Piece",
        1 => "Gram",
        2 => "Kilogram",
        3 => "Liter",
        4 => "Milliliter",
        5 => "Tablespoon",
        6 => "Teaspoon",
        7 => "Cup",
        8 => "Ounce",
        9 => "Pound",
        10 => "Pint",
        11 => "Quart",
        12 => "Gallon",
        _ => "Piece"
    };

    public static IReadOnlyList<int> AllUnits { get; } = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
}
