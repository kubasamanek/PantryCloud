namespace PantryCloud.Pantry.Application;

public interface IExpirationCheckService
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
