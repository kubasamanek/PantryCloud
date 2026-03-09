namespace PantryCloud.Load.NBomber.Utils;

public static class CorrelationIdProvider
{
    public static string Create() => Guid.NewGuid().ToString();
}

