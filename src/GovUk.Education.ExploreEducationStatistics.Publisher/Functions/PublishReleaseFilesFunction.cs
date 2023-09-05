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
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly IPublishingCompletionService _publishingCompletionService;

        public PublishReleaseFilesFunction(
            IPublishingService publishingService,
            IReleasePublishingStatusService releasePublishingStatusService, 
            IPublishingCompletionService publishingCompletionService)
        {
            _publishingService = publishingService;
            _releasePublishingStatusService = releasePublishingStatusService;
            _publishingCompletionService = publishingCompletionService;
        }

        /// <summary>
        /// Azure function which publishes the files for a Release by copying them between storage accounts.
        /// </summary>
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

            await UpdateFilesStage(message.Releases, Started);

            var successfulReleases = await message
                .Releases
                .ToAsyncEnumerable()
                .WhereAwait(async releaseStatus =>
                {
                    try
                    {
                        await _publishingService.PublishMethodologyFilesIfApplicableForRelease(releaseStatus.ReleaseId);
                        await _publishingService.PublishReleaseFiles(releaseStatus.ReleaseId);
                        return true;
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Exception occured while executing {FunctionName}",
                            executionContext.FunctionName);

                        return false;
                    }
                })
                .ToListAsync();

            var unsuccessfulReleases = message
                .Releases
                .Except(successfulReleases);

            await UpdateFilesStage(successfulReleases, Complete);
            await UpdateFilesStage(unsuccessfulReleases, Failed);

            try
            {
                await _publishingCompletionService.CompletePublishingIfAllPriorStagesComplete(successfulReleases);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured while completing publishing in {FunctionName}",
                    executionContext.FunctionName);
                
                await UpdatePublishingStage(
                    successfulReleases, 
                    ReleasePublishingStatusPublishingStage.Failed,
                    new ReleasePublishingStatusLogMessage($"Failed during completion of the Publishing process: {e.Message}"));
            }
            
            logger.LogInformation("{FunctionName} completed", executionContext.FunctionName);
        }

        private async Task UpdateFilesStage(
            IEnumerable<(Guid releaseId, Guid releaseStatusId)> releaseStatuses, 
            ReleasePublishingStatusFilesStage stage,
            ReleasePublishingStatusLogMessage logMessage = null)
        {
            await releaseStatuses
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(status => 
                    _releasePublishingStatusService.UpdateFilesStageAsync(
                        status.releaseId, 
                        status.releaseStatusId, 
                        stage, 
                        logMessage));
        }
        
        private async Task UpdatePublishingStage(
            IEnumerable<(Guid releaseId, Guid releaseStatusId)> releaseStatuses, 
            ReleasePublishingStatusPublishingStage stage,
            ReleasePublishingStatusLogMessage logMessage = null)
        {
            await releaseStatuses
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(status => 
                    _releasePublishingStatusService.UpdatePublishingStageAsync(
                        status.releaseId, 
                        status.releaseStatusId, 
                        stage, 
                        logMessage));
        }
    }
}
