#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public static class PublisherQueues
    {
        public const string StageReleaseContentQueue = "generate-release-content";
        public const string PublishMethodologyFilesQueue = "publish-methodology-files";
        public const string PublishReleaseContentQueue = "publish-release-content";
        public const string PublishReleaseFilesQueue = "publish-release-files";
        public const string PublishTaxonomyQueue = "publish-taxonomy";
        public const string RetryReleasePublishingQueue = "retry";
        public const string NotifyChangeQueue = "notify";
    }
}
