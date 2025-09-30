using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IQueueService
{
    Task QueueStageReleaseContentMessages(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys
    );

    Task QueuePublishReleaseContentMessage(ReleasePublishingKey releasePublishingKey);

    Task QueuePublishReleaseFilesMessages(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys
    );
}
