using Azure;
using Azure.Messaging.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

public interface IEventGridClient
{
    Task<Response?> SendEventAsync(EventGridEvent eventGridEvent, CancellationToken cancellationToken = default);
}
