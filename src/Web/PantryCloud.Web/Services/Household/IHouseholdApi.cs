namespace PantryCloud.Web.Services.Household;

public interface IHouseholdApi
{
    Task<GetCurrentHouseholdResponse?> GetCurrentHouseholdAsync(CancellationToken cancellationToken = default);
    Task<CreateHouseholdResponse?> CreateHouseholdAsync(CreateHouseholdRequest request, CancellationToken cancellationToken = default);
}
