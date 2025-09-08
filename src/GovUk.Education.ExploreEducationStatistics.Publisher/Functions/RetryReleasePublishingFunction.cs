using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class RetryReleasePublishingFunction(
    ILogger<RetryReleasePublishingFunction> logger,
    IQueueService queueService,
    IReleasePublishingStatusService releasePublishingStatusService
)
{
    private static readonly ReleasePublishingStatusOverallStage[] ValidStates =
    [
        ReleasePublishingStatusOverallStage.Complete,
        ReleasePublishingStatusOverallStage.Failed,
    ];

    /// <summary>
    /// BAU Azure function which retries the publishing of a Release by enqueueing a message to publish its
    /// content.  Note that this does not attempt to copy any Release files.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Function("RetryReleasePublishing")]
    public async Task RetryReleasePublishing(
        [QueueTrigger(RetryReleasePublishingQueue)] RetryReleasePublishingMessage message,
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var releaseStatus = await releasePublishingStatusService.GetLatest(message.ReleaseVersionId);

        if (releaseStatus == null)
        {
            logger.LogError(
                "Latest status not found for ReleaseVersion: {ReleaseVersionId} while attempting to retry",
                message.ReleaseVersionId
            );
        }
        else
        {
            if (!ValidStates.Contains(releaseStatus.State.Overall))
            {
                logger.LogError(
                    "Can only attempt a retry of ReleaseVersion: {ReleaseVersionId} if the latest "
                        + "status is in [{ValidStates}]. Found: {OverallState}",
                    message.ReleaseVersionId,
                    string.Join(", ", ValidStates),
                    releaseStatus.State.Overall
                );
            }
            else
            {
                await queueService.QueuePublishReleaseContentMessage(releaseStatus.AsTableRowKey());
            }
        }

        logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
    }
}
