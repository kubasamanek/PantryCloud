namespace PantryCloud.Web.Helpers;

/// <summary>
/// Display labels for household API enum values (Role and DietaryProfile are sent as int).
/// Backend: HouseholdRole 0=Owner, 1=Member; DietaryProfile 0=None, 1=Vegetarian, 2=Vegan.
/// </summary>
public static class HouseholdDisplayHelper
{
    public static string RoleLabel(int role) => role switch
    {
        0 => "Owner",
        1 => "Member",
        _ => "Member"
    };

    public static string DietaryProfileLabel(int dietaryProfile) => dietaryProfile switch
    {
        0 => "None",
        1 => "Vegetarian",
        2 => "Vegan",
        _ => "None"
    };
}
