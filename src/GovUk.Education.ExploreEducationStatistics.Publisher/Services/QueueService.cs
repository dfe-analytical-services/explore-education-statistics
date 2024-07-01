using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class QueueService(
    IStorageQueueService storageQueueService,
    IReleasePublishingStatusService releasePublishingStatusService,
    ILogger<QueueService> logger)
    : IQueueService
{
    public async Task QueueGenerateStagedReleaseContentMessage(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys)
    {
        logger.LogInformation(
            "Queuing generate content message for release versions: [{ReleaseVersionIds}]",
            releasePublishingKeys.Select(key => key.ReleaseVersionId).JoinToString(','));

        await storageQueueService.AddMessageAsync(
            StageReleaseContentQueue,
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
        await storageQueueService.AddMessageAsync(PublishReleaseContentQueue,
            new PublishReleaseContentMessage(releasePublishingKey));
        await releasePublishingStatusService.UpdateContentStage(releasePublishingKey,
            ReleasePublishingStatusContentStage.Queued);
    }

    public async Task QueuePublishReleaseFilesMessage(IReadOnlyList<ReleasePublishingKey> releasePublishingKeys)
    {
        logger.LogInformation(
            "Queuing files message for release versions: [{ReleaseVersionIds}]",
            releasePublishingKeys.Select(key => key.ReleaseVersionId).JoinToString(','));

        await storageQueueService.AddMessageAsync(PublishReleaseFilesQueue,
            new PublishReleaseFilesMessage(releasePublishingKeys));

        await releasePublishingKeys
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async key => await releasePublishingStatusService.UpdateFilesStage(key,
                ReleasePublishingStatusFilesStage.Queued));
    }
}
