using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusFilesStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class PublishReleaseFilesFunction(
        ILogger<PublishReleaseFilesFunction> logger,
        IPublishingService publishingService,
        IReleasePublishingStatusService releasePublishingStatusService,
        IPublishingCompletionService publishingCompletionService)
    {
        /// <summary>
        /// Azure function which publishes the files for a Release by copying them between storage accounts.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Function("PublishReleaseFiles")]
        public async Task PublishReleaseFiles(
            [QueueTrigger(PublishReleaseFilesQueue)]
            PublishReleaseFilesMessage message,
            FunctionContext context)
        {
            logger.LogInformation("{FunctionName} triggered: {Message}",
                context.FunctionDefinition.Name,
                message);

            await UpdateFilesStage(message.Releases, Started);

            var successfulReleases = await message
                .Releases
                .ToAsyncEnumerable()
                .WhereAwait(async releaseStatus =>
                {
                    try
                    {
                        await publishingService.PublishMethodologyFilesIfApplicableForRelease(
                            releaseStatus.ReleaseVersionId);
                        await publishingService.PublishReleaseFiles(releaseStatus.ReleaseVersionId);
                        return true;
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Exception occured while executing {FunctionName}",
                            context.FunctionDefinition.Name);

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
                await publishingCompletionService.CompletePublishingIfAllPriorStagesComplete(successfulReleases);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured while completing publishing in {FunctionName}",
                    context.FunctionDefinition.Name);

                await UpdatePublishingStage(
                    successfulReleases,
                    ReleasePublishingStatusPublishingStage.Failed,
                    new ReleasePublishingStatusLogMessage(
                        $"Failed during completion of the Publishing process: {e.Message}"));
            }

            logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
        }

        private async Task UpdateFilesStage(
            IEnumerable<(Guid releaseVersionId, Guid releaseStatusId)> releaseStatuses,
            ReleasePublishingStatusFilesStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await releaseStatuses
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(status =>
                    releasePublishingStatusService.UpdateFilesStageAsync(
                        releaseVersionId: status.releaseVersionId,
                        releaseStatusId: status.releaseStatusId,
                        stage,
                        logMessage));
        }

        private async Task UpdatePublishingStage(
            IEnumerable<(Guid releaseVersionId, Guid releaseStatusId)> releaseStatuses,
            ReleasePublishingStatusPublishingStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await releaseStatuses
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(status =>
                    releasePublishingStatusService.UpdatePublishingStageAsync(
                        releaseVersionId: status.releaseVersionId,
                        releaseStatusId: status.releaseStatusId,
                        stage,
                        logMessage));
        }
    }
}
