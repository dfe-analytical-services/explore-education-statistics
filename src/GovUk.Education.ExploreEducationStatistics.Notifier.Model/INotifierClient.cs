namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public interface INotifierClient
{
    public Task NotifyPublicationSubscribers(
        IReadOnlyList<ReleaseNotificationMessage> messages,
        CancellationToken cancellationToken = default
    );

    public Task NotifyApiSubscribers(
        IReadOnlyList<ApiNotificationMessage> messages,
        CancellationToken cancellationToken = default
    );

    public Task NotifyReleasePublishingFeedbackUsers(
        IReadOnlyList<ReleasePublishingFeedbackMessage> messages,
        CancellationToken cancellationToken = default
    );
}
