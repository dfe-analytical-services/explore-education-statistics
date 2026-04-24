namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public static class ReleasePublishingStatusStates
{
    /**
     * State used when a Release fails validation
     */
    public static readonly ReleasePublishingStatusState InvalidState = new(
        ReleasePublishingStatusFilesStage.Cancelled,
        ReleasePublishingStatusPublishingStage.Cancelled,
        ReleasePublishingStatusOverallStage.Invalid
    );

    /**
     * State used when a Release passes validation and is scheduled
     */
    public static readonly ReleasePublishingStatusState ScheduledState = new(
        ReleasePublishingStatusFilesStage.NotStarted,
        ReleasePublishingStatusPublishingStage.NotStarted,
        ReleasePublishingStatusOverallStage.Scheduled
    );

    /**
     * State used when the process of publishing a scheduled Release has started
     */
    public static readonly ReleasePublishingStatusState ScheduledReleaseStartedState = new(
        ReleasePublishingStatusFilesStage.NotStarted,
        ReleasePublishingStatusPublishingStage.Scheduled,
        ReleasePublishingStatusOverallStage.Started
    );

    /**
     * State used when the process of publishing an immediate Release has started
     */
    public static readonly ReleasePublishingStatusState ImmediateReleaseStartedState = new(
        ReleasePublishingStatusFilesStage.NotStarted,
        ReleasePublishingStatusPublishingStage.NotStarted,
        ReleasePublishingStatusOverallStage.Started
    );

    /**
     * State used when a newer request to publish a Release supersedes one already scheduled
     */
    public static readonly ReleasePublishingStatusState SupersededState = new(
        ReleasePublishingStatusFilesStage.Cancelled,
        ReleasePublishingStatusPublishingStage.Cancelled,
        ReleasePublishingStatusOverallStage.Superseded
    );
}
