using System.Net;
using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Events.Tests.EventGrid.Builders;

public class EventGridClientMockBuilder
{
    private readonly Mock<IEventGridClient> _mock = new(MockBehavior.Strict);
    private readonly List<EventGridEvent> _eventsPublished = new();
    private HttpStatusCode _httpStatusCode = HttpStatusCode.OK;
    private Exception? _sendEventAsyncException;

    public IEventGridClient Build()
    {
        if (_sendEventAsyncException is null)
        {
            _mock
                .Setup(m => m.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>()))
                .Callback((EventGridEvent eventGridEvent, CancellationToken cancellationToken) => _eventsPublished.Add(eventGridEvent))
                .ReturnsAsync(() => new MockResponse
                {
                    StatusCode = _httpStatusCode
                });
        }
        else
        {
            _mock
                .Setup(m => m.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(_sendEventAsyncException);
        }
        return _mock.Object;
    }

    public EventGridClientMockBuilder WhereResponseIs(HttpStatusCode statusCode)
    {
        _httpStatusCode = statusCode;
        return this;
    }

    public EventGridClientMockBuilder WhereSendEventAsyncThrows(Exception exception)
    {
        _sendEventAsyncException = exception;
        return this;
    }
    
    public Asserter Assert => new(_mock, _eventsPublished);
    public class Asserter(Mock<IEventGridClient> mock, List<EventGridEvent> eventsPublished)
    {
        public IEnumerable<EventGridEvent> EventsPublished => eventsPublished;
        
        public void NoEventsWerePublished() => mock.Verify(m => 
            m.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
