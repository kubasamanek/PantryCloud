namespace PantryCloud.Web.Helpers;

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
