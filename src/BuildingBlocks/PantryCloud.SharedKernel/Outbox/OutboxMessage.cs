namespace PantryCloud.SharedKernel.Outbox;

/// <summary>
/// Persisted representation of an integration event that must be reliably published to the message broker.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Assembly-qualified type name used to deserialize back to the concrete event.</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>JSON-serialized integration event body.</summary>
    public string Payload { get; init; } = string.Empty;

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    /// <summary>Set after successful broker publish.</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>Last error message from a failed publish attempt.</summary>
    public string? Error { get; set; }
}
