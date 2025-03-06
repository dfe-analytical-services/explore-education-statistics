namespace GovUk.Education.ExploreEducationStatistics.Publisher.Options;

public class EventGridOptions
{
    public const string Section = "EventGrid";

    public EventTopicOptions[] EventTopics { get; init; }
}

/// <summary>
/// Different event topic's configuration 
/// </summary>
public class EventTopicOptions
{
    public string Key { get; init; }
    public string TopicEndpoint { get; init; }
    public string? TopicAccessKey { get; init; }
}
