using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.EventGrid;

public class EventGridPublisherClientWrapper(EventGridPublisherClient eventGridPublisherClient)
    : IEventGridPublisherClient
{
    public async Task<Response> SendEventAsync(EventGridEvent eventGridEvent, CancellationToken cancellationToken = default) => 
        await eventGridPublisherClient.SendEventAsync(eventGridEvent, cancellationToken);
}