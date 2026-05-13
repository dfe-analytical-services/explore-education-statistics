using GovUk.Education.ExploreEducationStatistics.Publisher.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class QueueService(
    IPublisherClient publisherClient,
    IReleasePublishingStatusService releasePublishingStatusService,
    ILogger<QueueService> logger
) : IQueueService
{
    public async Task QueueReleaseFilesForImmediatePublishing(
        ReleasePublishingKey releasePublishingKey,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "Queuing files message for immediate publishing for release version: {ReleaseVersionId}",
            releasePublishingKey.ReleaseVersionId
        );

        await publisherClient.PublishReleaseFiles(
            PublishReleaseFilesMessage.ForImmediate(releasePublishingKey),
            cancellationToken
        );
        await UpdateFilesStageAsQueued([releasePublishingKey]);
    }

    public async Task QueueReleaseFilesForScheduledPublishing(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "Queuing files message for scheduled publishing for release versions: [{ReleaseVersionIds}]",
            releasePublishingKeys.ToReleaseVersionIdsString()
        );

        await publisherClient.PublishReleaseFiles(
            PublishReleaseFilesMessage.ForScheduled(releasePublishingKeys),
            cancellationToken
        );
        await UpdateFilesStageAsQueued(releasePublishingKeys);
    }

    private async Task UpdateFilesStageAsQueued(IReadOnlyList<ReleasePublishingKey> keys)
    {
        foreach (var key in keys)
        {
            await releasePublishingStatusService.UpdateFilesStage(key, ReleasePublishingStatusFilesStage.Queued);
        }
    }
}
