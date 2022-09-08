using System;
using System.Collections.Generic;
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
        // TODO DW - should we be completing the "all stages complete" step via a queue message?
        private readonly IQueueService _queueService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly IPublishingCompletionService _publishingCompletionService;

        public PublishReleaseFilesFunction(
            IPublishingService publishingService,
            IQueueService queueService,
            IReleasePublishingStatusService releasePublishingStatusService, 
            IPublishingCompletionService publishingCompletionService)
        {
            _publishingService = publishingService;
            _queueService = queueService;
            _releasePublishingStatusService = releasePublishingStatusService;
            _publishingCompletionService = publishingCompletionService;
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
            logger.LogInformation("{FunctionName} triggered: {Message}",
                executionContext.FunctionName,
                message);

            foreach (var (releaseId, releaseStatusId) in message.Releases)
            {
                try
                {
                    await UpdateStage(releaseId, releaseStatusId, Started);

                    await _publishingService.PublishMethodologyFilesIfApplicableForRelease(releaseId);
                    await _publishingService.PublishReleaseFiles(releaseId);
                    
                    await UpdateStage(releaseId, releaseStatusId, Complete);
                    
                    await _publishingCompletionService.CompletePublishingIfAllStagesComplete(
                        releaseId, 
                        releaseStatusId,
                        DateTime.UtcNow);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception occured while executing {FunctionName}",
                        executionContext.FunctionName);
                    logger.LogError("{StackTrace}", e.StackTrace);

                    await UpdateStage(releaseId, releaseStatusId, Failed,
                        new ReleasePublishingStatusLogMessage($"Exception in files stage: {e.Message}"));
                }
            }

            logger.LogInformation("{FunctionName} completed", executionContext.FunctionName);
        }

        private async Task UpdateStage(Guid releaseId, Guid releaseStatusId, ReleasePublishingStatusFilesStage stage,
            ReleasePublishingStatusLogMessage logMessage = null)
        {
            await _releasePublishingStatusService.UpdateFilesStageAsync(releaseId, releaseStatusId, stage, logMessage);
        }
    }
}
