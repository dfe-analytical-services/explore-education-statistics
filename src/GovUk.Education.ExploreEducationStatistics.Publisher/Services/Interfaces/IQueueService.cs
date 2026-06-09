using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IQueueService
{
    Task QueueReleaseFilesForImmediatePublishing(
        ReleasePublishingKey releasePublishingKey,
        CancellationToken cancellationToken = default
    );

    Task QueueReleaseFilesForScheduledPublishing(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys,
        CancellationToken cancellationToken = default
    );
}
