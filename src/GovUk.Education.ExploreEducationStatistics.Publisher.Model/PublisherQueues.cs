namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public static class PublisherQueues
    {
        public const string GenerateReleaseContentQueue = "generate-release-content";
        public const string PublishAllContentQueue = "publish-all-content";
        public const string PublishReleaseContentImmediateQueue = "publish-release-content-immediate";
        public const string PublishReleaseDataQueue = "publish-release-data";
        public const string PublishReleaseFilesQueue = "publish-release-files";
        public const string ValidateReleaseQueue = "releases";
    }
}