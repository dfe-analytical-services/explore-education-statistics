namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public interface INotifierClient
{
    // NOTE: If you want to send an email directly to GOVUK Notify rather than through EES.Notifier, use IEmailService
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
}
