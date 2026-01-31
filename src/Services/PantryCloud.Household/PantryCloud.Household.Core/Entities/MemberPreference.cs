using PantryCloud.Household.Core.Enums;

namespace PantryCloud.Household.Core.Entities;

public class MemberPreference
{
    public Guid UserId { get; set; }
    public DietaryProfile DietaryProfile { get; set; }
    public List<string> ExcludedIngredients { get; set; } = new();
}
