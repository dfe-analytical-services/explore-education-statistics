using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusFilesStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishReleaseFilesFunction
    {
        private readonly IPublishingService _publishingService;
        private readonly IQueueService _queueService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;

        public PublishReleaseFilesFunction(
            IPublishingService publishingService,
            IQueueService queueService,
            IReleasePublishingStatusService releasePublishingStatusService)
        {
            _publishingService = publishingService;
            _queueService = queueService;
            _releasePublishingStatusService = releasePublishingStatusService;
        }

        /// <summary>
        /// Azure function which publishes the files for a Release by copying them between storage accounts.
        /// </summary>
        /// <remarks>
        /// Triggers publishing statistics data for the Release if publishing is immediate.
        /// Triggers generating staged content for the Release if publishing is not immediate.
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("PublishReleaseFiles")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishReleaseFiles(
            [QueueTrigger(PublishReleaseFilesQueue)]
            PublishReleaseFilesMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered: {1}",
                executionContext.FunctionName,
                message);

            var immediate = await IsImmediate(message);
            var published = new List<(Guid ReleaseId, Guid ReleaseStatusId)>();
            foreach (var (releaseId, releaseStatusId) in message.Releases)
            {
                await UpdateStage(releaseId, releaseStatusId, Started);
                try
                {
                    await _publishingService.PublishMethodologyFilesIfApplicableForRelease(releaseId);
                    await _publishingService.PublishReleaseFiles(releaseId);
                    published.Add((releaseId, releaseStatusId));
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception occured while executing {0}",
                        executionContext.FunctionName);
                    logger.LogError("{StackTrace}", e.StackTrace);

                    await UpdateStage(releaseId, releaseStatusId, Failed,
                        new ReleasePublishingStatusLogMessage($"Exception in files stage: {e.Message}"));
                }
            }

            try
            {
                // TODO DW - EES-3369 - do we need this still?  Yeah probably, as this'll be triggered at midnight
                if (!immediate)
                {
                    await _queueService.QueueGenerateReleaseContentMessageAsync(published);
                }

                foreach (var (releaseId, releaseStatusId) in published)
                {
                    await UpdateStage(releaseId, releaseStatusId, Complete);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured while executing {0}",
                    executionContext.FunctionName);
                logger.LogError("{0}", e.StackTrace);
            }

            logger.LogInformation("{0} completed",
                executionContext.FunctionName);
        }

        private async Task<bool> IsImmediate(PublishReleaseFilesMessage message)
        {
            if (message.Releases.Count() > 1)
            {
                // If there's more than one Release this invocation couldn't have been triggered for immediate publishing
                return false;
            }

            var (releaseId, releaseStatusId) = message.Releases.Single();
            return await _releasePublishingStatusService.IsImmediate(releaseId, releaseStatusId);
        }

        private async Task UpdateStage(Guid releaseId, Guid releaseStatusId, ReleasePublishingStatusFilesStage stage,
            ReleasePublishingStatusLogMessage logMessage = null)
        {
            await _releasePublishingStatusService.UpdateFilesStageAsync(releaseId, releaseStatusId, stage, logMessage);
        }
    }
}
