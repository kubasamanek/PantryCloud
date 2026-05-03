using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Household.Application.Events;

public class OwnershipTransferredEvent : IntegrationEvent
{
    public Guid HouseholdId { get; init; }
    public Guid PreviousOwnerId { get; init; }
    public Guid NewOwnerId { get; init; }
    public string? NewOwnerEmail { get; init; }
    public DateTime TransferredAt { get; init; }
}
