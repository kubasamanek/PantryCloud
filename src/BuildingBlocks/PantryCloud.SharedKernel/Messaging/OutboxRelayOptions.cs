namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Configuration options for <see cref="OutboxRelayWorker{TDbContext}"/>.
/// </summary>
public class OutboxRelayOptions
{
    /// <summary>How often the relay worker polls for pending outbox messages. Default: 5 seconds.</summary>
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>Maximum number of messages to process per poll cycle. Default: 20.</summary>
    public int BatchSize { get; init; } = 20;
}
