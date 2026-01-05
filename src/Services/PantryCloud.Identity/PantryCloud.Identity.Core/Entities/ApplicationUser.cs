namespace PantryCloud.Identity.Core.Entities;

public class ApplicationUser
{
    public Guid Id { get; init; }
    
    public required string Email { get; init; }
    
    public required string PasswordHash { get; set; }
    public bool EmailVerified { get; set; }
    
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}
