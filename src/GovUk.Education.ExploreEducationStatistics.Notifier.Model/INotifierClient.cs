namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public interface INotifierClient
{
    Task NotifyPublicationSubscribers(
        IReadOnlyList<ReleaseNotificationMessage> messages,
        CancellationToken cancellationToken = default
    );

    Task NotifyApiSubscribers(
        IReadOnlyList<ApiNotificationMessage> messages,
        CancellationToken cancellationToken = default
    );

    Task NotifyReleasePublishingFeedbackUsers(
        IReadOnlyList<ReleasePublishingFeedbackMessage> messages,
        CancellationToken cancellationToken = default
    );

    Task NotifyEinTilesRequireUpdate(
        IReadOnlyList<EinTilesRequireUpdateMessage> messages,
        CancellationToken cancellationToken = default
    );
}
