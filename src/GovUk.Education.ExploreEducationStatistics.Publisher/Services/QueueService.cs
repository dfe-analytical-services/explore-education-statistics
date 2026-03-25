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
    public async Task QueuePublishReleaseFilesMessages(IReadOnlyList<ReleasePublishingKey> releasePublishingKeys)
    {
        logger.LogInformation(
            "Queuing files message for release versions: [{ReleaseVersionIds}]",
            releasePublishingKeys.ToReleaseVersionIdsString()
        );

        await publisherClient.PublishReleaseFiles(releasePublishingKeys);

        foreach (var key in releasePublishingKeys)
        {
            await releasePublishingStatusService.UpdateFilesStage(key, ReleasePublishingStatusFilesStage.Queued);
        }
    }
}
