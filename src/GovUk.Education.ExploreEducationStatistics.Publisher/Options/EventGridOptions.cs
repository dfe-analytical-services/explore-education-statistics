namespace GovUk.Education.ExploreEducationStatistics.Publisher.Options;

public class EventGridOptions
{
    public EventOptions[] EventOptions { get; init; }
}

public class EventOptions
{
    public string Key { get; init; }
    public string TopicEndpoint { get; init; }
    public string TopicName { get; init; }
    public string Subject { get; init; }
    public string EventType { get; init; }
}
