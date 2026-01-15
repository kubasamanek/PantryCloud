using ErrorOr;

namespace PantryCloud.SharedKernel.Messaging;

public static class EventPublishingExtensions
{
    public static async Task<ErrorOr<TResult>> PublishIfSuccessAsync<TResult, TEvent>(
        this ErrorOr<TResult> result,
        IMessageBus messageBus,
        Func<TResult, TEvent> eventFactory,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        if (result.IsError)
        {
            return result;
        }

        var metaDataEvent = eventFactory(result.Value);
        await messageBus.PublishAsync(metaDataEvent, cancellationToken);

        return result;
    }
}
