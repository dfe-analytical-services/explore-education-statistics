namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface INotificationsService
{
    Task NotifySubscribersIfApplicable(IReadOnlyList<Guid> releaseVersionIds);

    Task SendReleasePublishingFeedbackEmails(IReadOnlyList<Guid> releaseVersionIds);
}
