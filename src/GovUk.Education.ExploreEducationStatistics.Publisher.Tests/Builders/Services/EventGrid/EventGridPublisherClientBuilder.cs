using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.EventGrid;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services.EventGrid;

public class EventGridPublisherClientBuilder
{
    private readonly Mock<IEventGridPublisherClient> _mock = new(MockBehavior.Strict);
    private readonly List<EventGridEvent> _eventsPublished = new();
    private HttpStatusCode _httpStatusCode = HttpStatusCode.OK;
    private Exception? _sendEventAsyncException;

    public IEventGridPublisherClient Build()
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

    public EventGridPublisherClientBuilder WhereResponseIs(HttpStatusCode statusCode)
    {
        _httpStatusCode = statusCode;
        return this;
    }

    public EventGridPublisherClientBuilder WhereSendEventAsyncThrows(Exception exception)
    {
        _sendEventAsyncException = exception;
        return this;
    }
    
    public Asserter Assert => new(_mock, _eventsPublished);
    public class Asserter(Mock<IEventGridPublisherClient> mock, List<EventGridEvent> eventsPublished)
    {
        public IEnumerable<EventGridEvent> EventsPublished => eventsPublished;
        
        public void NoEventsWerePublished() => mock.Verify(m => 
            m.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
