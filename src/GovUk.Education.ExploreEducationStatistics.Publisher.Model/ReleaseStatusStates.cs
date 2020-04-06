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
         * State used when the process of publishing a Release has started
         */
        public static readonly ReleaseStatusState StartedState =
            new ReleaseStatusState(ReleaseStatusContentStage.NotStarted,
                ReleaseStatusFilesStage.NotStarted,
                ReleaseStatusDataStage.NotStarted,
                ReleaseStatusPublishingStage.Scheduled,
                ReleaseStatusOverallStage.Started);
        
        /**
         * TODO BAU-541 Combine with the above and make pubishing state notstarted?
         */
        public static readonly ReleaseStatusState StartedImmediateState =
            new ReleaseStatusState(ReleaseStatusContentStage.NotStarted,
                ReleaseStatusFilesStage.NotStarted,
                ReleaseStatusDataStage.NotStarted,
                ReleaseStatusPublishingStage.ToDo,
                ReleaseStatusOverallStage.Started);
    }
}