namespace GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;

/// <summary>
/// Different event topic's configuration 
/// </summary>
public class EventTopicOptions
{
    public required string Key { get; init; }
    public required string TopicEndpoint { get; init; }
    public string? TopicAccessKey { get; init; }
}