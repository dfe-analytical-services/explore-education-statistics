using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public class NotifierClient(string connectionString) : INotifierClient
{
    private readonly QueueServiceClient _queueServiceClient = new(connectionString);

    public async Task NotifyPublicationSubscribers(
        IReadOnlyList<ReleaseNotificationMessage> messages, 
        CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessagesAsJson(NotifierQueues.ReleaseNotificationQueue,
            messages,
            cancellationToken);
    }

    public async Task NotifyApiSubscribers(
        IReadOnlyList<ApiNotificationMessage> messages, 
        CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessagesAsJson(NotifierQueues.ApiNotificationQueue,
            messages,
            cancellationToken);
    }
}
