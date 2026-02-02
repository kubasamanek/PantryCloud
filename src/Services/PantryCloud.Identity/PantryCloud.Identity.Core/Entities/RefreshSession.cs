namespace PantryCloud.Identity.Core.Entities;

public class RefreshSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public string? DeviceName { get; set; }

    public ApplicationUser? User { get; set; }
}
