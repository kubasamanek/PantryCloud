namespace PantryCloud.SharedKernel.Outbox;

/// <summary>
/// Infrastructure-level record of a successfully processed incoming integration event.
/// </summary>
public class ProcessedInboxMessage
{
    public Guid MessageId { get; init; }

    public string EventType { get; init; } = string.Empty;

    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
}
