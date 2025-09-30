using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.Core;

public class EventGridEventHandlerTests
{
    private readonly FunctionContextMockBuilder _functionContextMockBuilder = new();
    private readonly EventGridEventBuilder _eventGridEventBuilder = new();
    private readonly LoggerMockBuilder<EventGridEventHandler> _loggerMockBuilder = new();

    private EventGridEventHandler GetSut() => new(_loggerMockBuilder.Build());

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenEvent_WhenHasPayload_ThenHandlerIsCalledWithPayload()
    {
        // ARRANGE
        var expectedPayload = new MockPayloadClass { Messaage = "This is the payload" };
        _eventGridEventBuilder.WithPayload(expectedPayload);
        var sut = GetSut();

        MockPayloadClass? actualPayload = null;

        Task<MockResponse> Handler(
            MockPayloadClass payload,
            CancellationToken cancellationToken = default
        )
        {
            actualPayload = payload;
            return Task.FromResult(new MockResponse());
        }

        // ACT
        await sut.Handle<MockPayloadClass, MockResponse>(
            _functionContextMockBuilder.Build(),
            _eventGridEventBuilder.Build(),
            Handler
        );

        // ASSERT
        Assert.NotNull(actualPayload);
        Assert.Equal(expectedPayload.Messaage, actualPayload.Messaage);
    }

    [Fact]
    public async Task GivenEvent_WhenHasRecordPayload_ThenHandlerIsCalledWithPayload()
    {
        // ARRANGE
        var expectedPayload = new MockPayloadRecord { Messaage = "This is the payload" };
        _eventGridEventBuilder.WithPayload(expectedPayload);
        var sut = GetSut();

        MockPayloadRecord? actualPayload = null;

        Task<MockResponse> Handler(
            MockPayloadRecord payload,
            CancellationToken cancellationToken = default
        )
        {
            actualPayload = payload;
            return Task.FromResult(new MockResponse());
        }

        // ACT
        await sut.Handle<MockPayloadRecord, MockResponse>(
            _functionContextMockBuilder.Build(),
            _eventGridEventBuilder.Build(),
            Handler
        );

        // ASSERT
        Assert.Equal(expectedPayload, actualPayload);
    }

    [Fact]
    public async Task GivenEvent_WhenEventHandled_ThenHandlerLogs()
    {
        // ARRANGE
        _functionContextMockBuilder.ForFunctionName("FUNCTION_NAME_123");
        var sut = GetSut();

        Task<MockResponse> Handler(
            MockPayloadRecord payload,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(new MockResponse());
        }

        // ACT
        await sut.Handle<MockPayloadRecord, MockResponse>(
            _functionContextMockBuilder.Build(),
            _eventGridEventBuilder.Build(),
            Handler
        );

        // ASSERT
        _loggerMockBuilder.Assert.LoggedDebugContains("FUNCTION_NAME_123");
    }

    [Fact]
    public async Task GivenEvent_WhenHasNoPayload_ThenHandlerIsCalledWithDefaultPayload()
    {
        // ARRANGE
        _eventGridEventBuilder.WithPayload(new object());
        var sut = GetSut();

        MockPayloadClass? actualPayload = null;

        Task<MockResponse> Handler(
            MockPayloadClass payload,
            CancellationToken cancellationToken = default
        )
        {
            actualPayload = payload;
            return Task.FromResult(new MockResponse());
        }

        // ACT
        await sut.Handle<MockPayloadClass, MockResponse>(
            _functionContextMockBuilder.Build(),
            _eventGridEventBuilder.Build(),
            Handler
        );

        // ASSERT
        Assert.NotNull(actualPayload);
        Assert.Null(actualPayload.Messaage);
    }

    [Fact]
    public async Task GivenEvent_WhenHasNoRecordPayload_ThenHandlerIsCalledWithDefaultPayload()
    {
        // ARRANGE
        _eventGridEventBuilder.WithPayload(new object());
        var sut = GetSut();

        MockPayloadRecord? actualPayload = null;

        Task<MockResponse> Handler(
            MockPayloadRecord payload,
            CancellationToken cancellationToken = default
        )
        {
            actualPayload = payload;
            return Task.FromResult(new MockResponse());
        }

        // ACT
        await sut.Handle<MockPayloadRecord, MockResponse>(
            _functionContextMockBuilder.Build(),
            _eventGridEventBuilder.Build(),
            Handler
        );

        // ASSERT
        var expectedPayload = new MockPayloadRecord();
        Assert.Equal(expectedPayload, actualPayload);
    }

    [Fact]
    public async Task GivenHandlerCalled_WhenHandlerReturnsResponse_ThenSUTReturnsResponse()
    {
        // ARRANGE
        var sut = GetSut();

        var expectedResponse = new MockResponse();

        Task<MockResponse> Handler(
            MockPayloadClass payload,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(expectedResponse);
        }

        // ACT
        var actualReponse = await sut.Handle<MockPayloadClass, MockResponse>(
            _functionContextMockBuilder.Build(),
            _eventGridEventBuilder.Build(),
            Handler
        );

        // ASSERT
        Assert.NotNull(actualReponse);
        Assert.Equal(expectedResponse, actualReponse);
    }

    public class MockPayloadClass
    {
        public string? Messaage { get; init; }
    }

    public record MockPayloadRecord
    {
        public string? Messaage { get; init; }
    }

    public record MockResponse { }
}
