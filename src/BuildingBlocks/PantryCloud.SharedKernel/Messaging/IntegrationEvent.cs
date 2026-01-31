namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Base class for all integration events.
/// Integration events represent something that has happened in the system and other services might be interested in.
/// </summary>
public abstract class IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    public string? CorrelationId { get; init; }
}
