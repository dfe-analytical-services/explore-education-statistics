using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class QueueService(
    IPublisherQueueServiceClient publisherQueueServiceClient,
    IReleasePublishingStatusService releasePublishingStatusService,
    ILogger<QueueService> logger)
    : IQueueService
{
    public async Task QueueStageReleaseContentMessages(IReadOnlyList<ReleasePublishingKey> releasePublishingKeys)
    {
        logger.LogInformation(
            "Queuing generate content message for release versions: [{ReleaseVersionIds}]",
            releasePublishingKeys.Select(key => key.ReleaseVersionId).JoinToString(','));

        await publisherQueueServiceClient.SendMessageAsJson(
            PublisherQueues.StageReleaseContentQueue,
            new StageReleaseContentMessage(releasePublishingKeys));

        await releasePublishingKeys
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async key =>
                await releasePublishingStatusService.UpdateContentStage(key,
                    ReleasePublishingStatusContentStage.Queued));
    }

    public async Task QueuePublishReleaseContentMessage(ReleasePublishingKey releasePublishingKey)
    {
        logger.LogInformation("Queuing publish content message for release version: {ReleaseVersionId}",
            releasePublishingKey.ReleaseVersionId);
        await publisherQueueServiceClient.SendMessageAsJson(PublisherQueues.PublishReleaseContentQueue,
            new PublishReleaseContentMessage(releasePublishingKey));
        await releasePublishingStatusService.UpdateContentStage(releasePublishingKey,
            ReleasePublishingStatusContentStage.Queued);
    }

    public async Task QueuePublishReleaseFilesMessages(IReadOnlyList<ReleasePublishingKey> releasePublishingKeys)
    {
        logger.LogInformation(
            "Queuing files message for release versions: [{ReleaseVersionIds}]",
            releasePublishingKeys.Select(key => key.ReleaseVersionId).JoinToString(','));

        await publisherQueueServiceClient.SendMessageAsJson(PublisherQueues.PublishReleaseFilesQueue,
            new PublishReleaseFilesMessage(releasePublishingKeys));

        await releasePublishingKeys
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async key => await releasePublishingStatusService.UpdateFilesStage(key,
                ReleasePublishingStatusFilesStage.Queued));
    }
}
