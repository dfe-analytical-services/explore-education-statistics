namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public static class ReleaseStatusStates
    {
        /**
         * State used when a Release fails validation
         */
        public static readonly ReleaseStatusState InvalidState =
            new ReleaseStatusState(ReleaseStatusContentStage.Cancelled,
                ReleaseStatusFilesStage.Cancelled,
                ReleaseStatusDataStage.Cancelled,
                ReleaseStatusPublishingStage.Cancelled,
                ReleaseStatusOverallStage.Invalid);

        /**
         * State used when a Release passes validation and is scheduled
         */
        public static readonly ReleaseStatusState ScheduledState =
            new ReleaseStatusState(ReleaseStatusContentStage.NotStarted,
                ReleaseStatusFilesStage.NotStarted,
                ReleaseStatusDataStage.NotStarted,
                ReleaseStatusPublishingStage.NotStarted,
                ReleaseStatusOverallStage.Scheduled);

        /**
         * State used when the process of publishing a scheduled Release has started
         */
        public static readonly ReleaseStatusState ScheduledReleaseStartedState =
            new ReleaseStatusState(ReleaseStatusContentStage.NotStarted,
                ReleaseStatusFilesStage.NotStarted,
                ReleaseStatusDataStage.NotStarted,
                ReleaseStatusPublishingStage.Scheduled,
                ReleaseStatusOverallStage.Started);
        
        /**
         * State used when the process of publishing an immediate Release has started
         */
        public static readonly ReleaseStatusState ImmediateReleaseStartedState =
            new ReleaseStatusState(ReleaseStatusContentStage.NotStarted,
                ReleaseStatusFilesStage.NotStarted,
                ReleaseStatusDataStage.NotStarted,
                ReleaseStatusPublishingStage.NotStarted,
                ReleaseStatusOverallStage.Started);
    }
}