namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model
{
    public static class ImporterQueues
    {
        public const string ImportsCancellingQueue = "imports-cancelling";
        public const string ImportsAvailableQueue = "imports-available";
        public const string ImportsAvailablePoisonQueue = "imports-available-poison";
        public const string ImportsPendingQueue = "imports-pending";
        public const string ImportsPendingPoisonQueue = "imports-pending-poison";
        public const string RestartImportsQueue = "restart-imports";
    }
}