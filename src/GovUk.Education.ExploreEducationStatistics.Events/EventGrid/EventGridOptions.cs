namespace GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

public class EventGridOptions
{
    public const string Section = "EventGrid";

    public EventTopicOptions[] EventTopics { get; init; } = [];
}
