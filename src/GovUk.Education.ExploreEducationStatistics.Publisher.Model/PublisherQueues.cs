#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public static class PublisherQueues
    {
        public const string GenerateReleaseContentQueue = "generate-release-content";
        public const string PublishAllContentQueue = "publish-all-content";
        public const string PublishMethodologyFilesQueue = "publish-methodology-files";
        public const string PublishReleaseContentQueue = "publish-release-content";
        public const string PublishReleaseDataQueue = "publish-release-data";
        public const string PublishReleaseFilesQueue = "publish-release-files";
        public const string PublishTaxonomyQueue = "publish-taxonomy";
        public const string RetryStageQueue = "retry";
        public const string NotifyChangeQueue = "notify";

        /**
         * Temporary queue used to migrate files in EES-3547
         * TODO Remove in EES-3552
         */
        public const string MigrateFilesQueue = "migrate-files";
    }
}
