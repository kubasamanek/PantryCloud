namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Configuration options for <see cref="OutboxRelayWorker{TDbContext}"/>.
/// </summary>
public class OutboxRelayOptions
{
    /// <summary>How often the relay worker polls for pending outbox messages. Default: 2 seconds (for lower replication lag).</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>Maximum number of messages to process per poll cycle. Default: 20.</summary>
    public int BatchSize { get; set; } = 20;
}
