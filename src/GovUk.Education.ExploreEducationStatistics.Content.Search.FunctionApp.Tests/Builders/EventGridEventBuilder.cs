using Azure.Messaging.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class EventGridEventBuilder
{
    private object _payload = new();
    private string? _subject;

    public EventGridEvent Build()
    {
        return new EventGridEvent(_subject ?? "Event Subject", "Event Type", "1.1", _payload);
    }

    public EventGridEventBuilder WithPayload(object payload)
    {
        _payload = payload;
        return this;
    }

    public EventGridEventBuilder WithSubject(string subject)
    {
        _subject = subject;
        return this;
    }
}
