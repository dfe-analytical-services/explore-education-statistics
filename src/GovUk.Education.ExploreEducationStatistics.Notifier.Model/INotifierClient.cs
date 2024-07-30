using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public interface INotifierClient
{
    public Task NotifyPublicationSubscribers(
        IReadOnlyList<ReleaseNotificationMessage> messages, CancellationToken cancellationToken = default);

    public Task NotifyApiSubscribers(
        IReadOnlyList<ApiNotificationMessage> messages, CancellationToken cancellationToken = default);
}
