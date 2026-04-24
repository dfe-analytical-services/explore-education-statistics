#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public static class PublisherQueues
{
    public const string PublishMethodologyFilesQueue = "publish-methodology-files";
    public const string PublishReleaseFilesQueue = "publish-release-files";
    public const string PublishTaxonomyQueue = "publish-taxonomy";
    public const string NotifyChangeQueue = "notify";
}
