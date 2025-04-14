using Azure.Messaging.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;

public interface IEvent
{
    static abstract string EventTopicOptionsKey { get; }
    EventGridEvent ToEventGridEvent();
}
