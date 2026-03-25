using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IQueueService
{
    Task QueuePublishReleaseFilesMessages(IReadOnlyList<ReleasePublishingKey> releasePublishingKeys);
}
