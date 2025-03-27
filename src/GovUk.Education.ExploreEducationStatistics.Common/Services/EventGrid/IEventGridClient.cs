#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;

public interface IEventGridClient
{
    Task<Response?> SendEventAsync(EventGridEvent eventGridEvent, CancellationToken cancellationToken = default);
}
