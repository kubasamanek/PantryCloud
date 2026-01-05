namespace PantryCloud.Household.Application;

public interface IUserContext
{
    Guid UserId { get; }
    string Email { get; }
}