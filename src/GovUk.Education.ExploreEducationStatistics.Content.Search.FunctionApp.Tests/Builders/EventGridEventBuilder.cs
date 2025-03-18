using Azure.Messaging.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class EventGridEventBuilder
{
    private object _payload = new();
    
    public EventGridEvent Build()
    {
        return new EventGridEvent(
            "Event Subject",
            "Event Type",
            "1.1",
            _payload);
    }

    public EventGridEventBuilder WithPayload(object payload)
    {
        _payload = payload;
        return this;
    }
}
