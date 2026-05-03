using PantryCloud.Load.NBomber.Clients;

namespace PantryCloud.Load.NBomber.Flows;

public static class DomainFlows
{
    public static async Task EnsureHouseholdWithPantryAsync(
        AuthContext authContext,
        HouseholdClient householdClient,
        PantryClient pantryClient,
        long invocationNumber,
        int iteration,
        CancellationToken cancellationToken)
    {
        var householdName = $"LoadTest HH {invocationNumber} iter{iteration}";

        await householdClient.CreateHouseholdAsync(
            authContext.AccessToken,
            householdName,
            cancellationToken);

        await householdClient.GetCurrentHouseholdAsync(
            authContext.AccessToken,
            cancellationToken);

        await pantryClient.ListItemsAsync(
            authContext.AccessToken,
            cancellationToken);

        var itemName = $"LoadItem {invocationNumber} i{iteration}";

        await pantryClient.CreateItemAsync(
            authContext.AccessToken,
            itemName,
            1,
            unit: 0,
            cancellationToken);
    }
}

