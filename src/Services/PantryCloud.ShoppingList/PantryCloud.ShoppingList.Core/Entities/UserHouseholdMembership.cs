using PantryCloud.SharedKernel.Entities;

namespace PantryCloud.ShoppingList.Core.Entities;

public class UserHouseholdMembership : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid HouseholdId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}


