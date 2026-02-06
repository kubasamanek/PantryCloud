namespace PantryCloud.Audit.Core.Options;

public class AuditOptions
{
    public const string SectionName = "Audit";

    public int RetentionDays { get; set; } = 30;
}
