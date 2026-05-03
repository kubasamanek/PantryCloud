namespace PantryCloud.E2E.Runner.Core;

public static class TestIdBuilder
{
    public static string IdentityEmail(Random random) =>
        UniqueEmail("identity", random);

    public static string UserEmail(Random random) =>
        UniqueEmail("user", random);

    public static string HouseholdOwnerEmail(Random random) =>
        UniqueEmail("owner", random);

    public static string HouseholdMemberEmail(Random random) =>
        UniqueEmail("member", random);

    public static string UniqueEmail(string prefix, Random random)
    {
        var ticks = DateTime.UtcNow.Ticks;
        var suffix = random.Next(1000, 9999);
        return $"{prefix}_{ticks}_{suffix}@test.pantrycloud.local";
    }
}

