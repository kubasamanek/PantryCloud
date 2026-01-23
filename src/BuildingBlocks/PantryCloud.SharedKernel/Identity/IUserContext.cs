namespace PantryCloud.SharedKernel.Identity;

public interface IUserContext
{
    Guid UserId { get; }
    string Email { get; }
}

