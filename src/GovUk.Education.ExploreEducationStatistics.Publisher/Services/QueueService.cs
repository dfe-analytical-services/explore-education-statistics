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
    public async Task QueueStageReleaseContentMessages(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys
    )
    {
        logger.LogInformation(
            "Queuing generate content message for release versions: [{ReleaseVersionIds}]",
            releasePublishingKeys.ToReleaseVersionIdsString()
        );

        await publisherClient.StageReleaseContent(releasePublishingKeys);

        await releasePublishingKeys
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async key =>
                await releasePublishingStatusService.UpdateContentStage(
                    key,
                    ReleasePublishingStatusContentStage.Queued
                )
            );
    }

    public async Task QueuePublishReleaseContentMessage(ReleasePublishingKey releasePublishingKey)
    {
        logger.LogInformation(
            "Queuing publish content message for release version: {ReleaseVersionId}",
            releasePublishingKey.ReleaseVersionId
        );
        await publisherClient.PublishReleaseContent(releasePublishingKey);
        await releasePublishingStatusService.UpdateContentStage(
            releasePublishingKey,
            ReleasePublishingStatusContentStage.Queued
        );
    }

    public async Task QueuePublishReleaseFilesMessages(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys
    )
    {
        logger.LogInformation(
            "Queuing files message for release versions: [{ReleaseVersionIds}]",
            releasePublishingKeys.ToReleaseVersionIdsString()
        );

        await publisherClient.PublishReleaseFiles(releasePublishingKeys);

        await releasePublishingKeys
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async key =>
                await releasePublishingStatusService.UpdateFilesStage(
                    key,
                    ReleasePublishingStatusFilesStage.Queued
                )
            );
    }
}
