using Azure.Messaging.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

public interface IEvent
{
    static abstract string EventTopicOptionsKey { get; }
    EventGridEvent ToEventGridEvent();
}
