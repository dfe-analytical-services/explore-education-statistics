using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class RetryReleasePublishingFunction
    {
        private readonly IQueueService _queueService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;

        private static readonly ReleasePublishingStatusOverallStage[] ValidStates =
        {
            ReleasePublishingStatusOverallStage.Complete, ReleasePublishingStatusOverallStage.Failed
        };

        public RetryReleasePublishingFunction(
            IQueueService queueService,
            IReleasePublishingStatusService releasePublishingStatusService)
        {
            _queueService = queueService;
            _releasePublishingStatusService = releasePublishingStatusService;
        }

        /// <summary>
        /// BAU Azure function which retries the publishing of a Release by enqueueing a message to publish its
        /// content.  Note that this does not attempt to copy any Release files.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("RetryReleasePublishing")]
        // ReSharper disable once UnusedMember.Global
        public async Task RetryReleasePublishing(
            [QueueTrigger(RetryReleasePublishingQueue)]
            RetryReleasePublishingMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered at: {1}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            var releaseStatus = await _releasePublishingStatusService.GetLatestAsync(message.ReleaseId);

            if (releaseStatus == null)
            {
                logger.LogError(
                    "Latest status not found for Release: {0} while attempting to retry",
                    message.ReleaseId);
            }
            else
            {
                if (!ValidStates.Contains(releaseStatus.State.Overall))
                {
                    logger.LogError("Can only attempt a retry of Release: {0} if the latest " +
                                    "status is in ({1}). Found: {2}",
                        message.ReleaseId,
                        string.Join(", ", ValidStates),
                        releaseStatus.State.Overall);
                }
                else
                {
                    await _queueService.QueuePublishReleaseContentMessage(message.ReleaseId, releaseStatus.Id);
                }
            }

            logger.LogInformation("{0} completed", executionContext.FunctionName);
        }
    }
}
