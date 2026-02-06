namespace PantryCloud.Audit.Core.Entities;

public class HouseholdAuditEntry
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public string? Payload { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? CorrelationId { get; set; }
    public Guid EventId { get; set; }
}
