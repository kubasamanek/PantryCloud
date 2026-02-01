using PantryCloud.SharedKernel.Entities;

namespace PantryCloud.Notification.Core.Entities;

public class UserHouseholdMembership : BaseEntity
{
    public Guid UserId { get; init; }
    public Guid HouseholdId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }

    public bool IsActive => LeftAt == null;
}
