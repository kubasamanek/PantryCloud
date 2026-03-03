using ErrorOr;
using NSubstitute;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Messaging;

public class EventPublishingExtensionsTests
{
    private readonly IOutboxWriter _outboxWriter = Substitute.For<IOutboxWriter>();
    private readonly ICorrelationIdProvider _correlationIdProvider = Substitute.For<ICorrelationIdProvider>();

    [Fact]
    public async Task WriteToOutboxIfSuccessAsync_ShouldWriteEvent_WhenResultIsSuccess()
    {
        _correlationIdProvider.GetCorrelationId().Returns(Constants.Outbox.CorrelationId);
        ErrorOr<string> result = Constants.Outbox.SuccessValue;

        await result.WriteToOutboxIfSuccessAsync(
            _outboxWriter,
            (value, correlationId) => new TestIntegrationEvent { Data = value, CorrelationId = correlationId },
            _correlationIdProvider);

        await _outboxWriter.Received(1).WriteAsync(
            Arg.Is<TestIntegrationEvent>(e =>
                e.Data == Constants.Outbox.SuccessValue &&
                e.CorrelationId == Constants.Outbox.CorrelationId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WriteToOutboxIfSuccessAsync_ShouldNotWriteEvent_WhenResultIsError()
    {
        ErrorOr<string> result = Error.Failure("Test.Error", "Test error");

        await result.WriteToOutboxIfSuccessAsync(
            _outboxWriter,
            (value, correlationId) => new TestIntegrationEvent { Data = value, CorrelationId = correlationId },
            _correlationIdProvider);

        await _outboxWriter.DidNotReceive().WriteAsync(
            Arg.Any<TestIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WriteToOutboxIfSuccessAsync_ShouldReturnOriginalResult_WhenResultIsSuccess()
    {
        _correlationIdProvider.GetCorrelationId().Returns(Constants.Outbox.CorrelationId);
        ErrorOr<string> result = Constants.Outbox.SuccessValue;

        var returned = await result.WriteToOutboxIfSuccessAsync(
            _outboxWriter,
            (value, correlationId) => new TestIntegrationEvent { Data = value, CorrelationId = correlationId },
            _correlationIdProvider);

        returned.IsError.ShouldBeFalse();
        returned.Value.ShouldBe(Constants.Outbox.SuccessValue);
    }

    [Fact]
    public async Task WriteToOutboxIfSuccessAsync_ShouldReturnOriginalError_WhenResultIsError()
    {
        var error = Error.NotFound("Test.NotFound", "Not found");
        ErrorOr<string> result = error;

        var returned = await result.WriteToOutboxIfSuccessAsync(
            _outboxWriter,
            (value, correlationId) => new TestIntegrationEvent { Data = value, CorrelationId = correlationId },
            _correlationIdProvider);

        returned.IsError.ShouldBeTrue();
        returned.FirstError.Code.ShouldBe(error.Code);
    }

    private class TestIntegrationEvent : IntegrationEvent
    {
        public string Data { get; init; } = string.Empty;
    }
}
