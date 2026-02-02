using PantryCloud.SharedKernel.Entities;

namespace PantryCloud.Identity.Core.Entities;

public class ApplicationUser : BaseEntity
{
    public required string Email { get; init; }
    
    public required string PasswordHash { get; set; }
    
    public bool EmailVerified { get; set; }
}
