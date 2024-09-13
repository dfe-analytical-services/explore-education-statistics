using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class QueueService(
    IPublisherClient publisherClient,
    IReleasePublishingStatusService releasePublishingStatusService,
    ILogger<QueueService> logger)
    : IQueueService
{
    public async Task QueueStageReleaseContentMessages(IReadOnlyList<ReleasePublishingKeyOld> releasePublishingKeys)
    {
        logger.LogInformation(
            "Queuing generate content message for release versions: [{ReleaseVersionIds}]",
            releasePublishingKeys.Select(key => key.ReleaseVersionId).JoinToString(','));

        await publisherClient.StageReleaseContent(releasePublishingKeys);

        await releasePublishingKeys
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async key =>
                await releasePublishingStatusService.UpdateContentStage(key,
                    ReleasePublishingStatusContentStage.Queued));
    }

    public async Task QueuePublishReleaseContentMessage(ReleasePublishingKeyOld releasePublishingKeyOld)
    {
        logger.LogInformation("Queuing publish content message for release version: {ReleaseVersionId}",
            releasePublishingKeyOld.ReleaseVersionId);
        await publisherClient.PublishReleaseContent(releasePublishingKeyOld);
        await releasePublishingStatusService.UpdateContentStage(releasePublishingKeyOld,
            ReleasePublishingStatusContentStage.Queued);
    }

    public async Task QueuePublishReleaseFilesMessages(IReadOnlyList<ReleasePublishingKeyOld> releasePublishingKeys)
    {
        logger.LogInformation(
            "Queuing files message for release versions: [{ReleaseVersionIds}]",
            releasePublishingKeys.Select(key => key.ReleaseVersionId).JoinToString(','));

        await publisherClient.PublishReleaseFiles(releasePublishingKeys);

        await releasePublishingKeys
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async keyOld =>
            {
                var key = new ReleasePublishingKey(keyOld.ReleaseVersionId, keyOld.ReleaseVersionId);
                await releasePublishingStatusService.UpdateFilesStage(key,
                    ReleasePublishingStatusFilesStage.Queued);
            });
    }
}
