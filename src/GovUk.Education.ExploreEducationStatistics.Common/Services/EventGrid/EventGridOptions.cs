namespace GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;

public class EventGridOptions
{
    public const string Section = "EventGrid";

    public EventTopicOptions[] EventTopics { get; init; } = [];
}
