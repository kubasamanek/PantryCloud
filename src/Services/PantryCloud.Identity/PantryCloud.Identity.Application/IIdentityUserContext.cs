namespace PantryCloud.Identity.Application;

public interface IIdentityUserContext
{
    Guid UserId { get; }
    string Email { get; }
    Guid? SessionId { get; }
}
