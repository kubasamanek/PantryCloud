namespace PantryCloud.Household.Core.Entities;

public class MemberProfile
{
    public Guid UserId { get; set; }
    public required string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
}
