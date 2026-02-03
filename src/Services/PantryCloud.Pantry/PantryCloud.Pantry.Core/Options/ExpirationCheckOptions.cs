namespace PantryCloud.Pantry.Core.Options;

public class ExpirationCheckOptions
{
    public const string SectionName = "ExpirationCheck";

    public bool Enabled { get; set; } = true;
    public bool DebugEnabled { get; set; } = false;
    public int IntervalHours { get; set; } = 24;
    public int BatchSize { get; set; } = 50;
    public int BatchDelayMs { get; set; } = 100;
}
