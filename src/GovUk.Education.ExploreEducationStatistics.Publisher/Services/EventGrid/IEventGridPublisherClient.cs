using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.EventGrid;

public interface IEventGridPublisherClient
{
    Task<Response> SendEventAsync(EventGridEvent eventGridEvent, CancellationToken cancellationToken = default);
}
