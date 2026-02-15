using PantryCloud.SharedKernel.Entities;

namespace PantryCloud.Identity.Core.Entities;

public class ResetPasswordToken : BaseEntity
{
    public required string Email { get; init; }
    public DateTime ExpiresAt { get; init; }
    public required string Token { get; init; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string CallBackUrl { get; init; } = string.Empty;

    public bool IsUsed => UsedAt.HasValue;
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}