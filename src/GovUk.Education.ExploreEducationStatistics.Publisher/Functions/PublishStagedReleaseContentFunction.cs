#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusPublishingStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishStagedReleaseContentFunction
    {
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly IPublishingService _publishingService;
        private readonly IPublishingCompletionService _publishingCompletionService;

        public PublishStagedReleaseContentFunction(
            IReleasePublishingStatusService releasePublishingStatusService,
            IPublishingService publishingService,
            IPublishingCompletionService publishingCompletionService)
        {
            _releasePublishingStatusService = releasePublishingStatusService;
            _publishingService = publishingService;
            _publishingCompletionService = publishingCompletionService;
        }

        /// <summary>
        /// Azure function which publishes the content for a Release at a scheduled time by moving it from a staging directory.
        /// </summary>
        /// <remarks>
        /// Sets the published time on the Release which means it's considered as 'Live'.
        /// </remarks>
        /// <param name="timer"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("PublishStagedReleaseContent")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishStagedReleaseContent([TimerTrigger("%PublishReleaseContentCronSchedule%")]
            TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered at: {1}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            var scheduled = (await QueryScheduledReleases()).ToList();
            if (scheduled.Any())
            {

                await UpdateStage(scheduled, Started);

                // Move all cached releases in the staging directory of the public content container to the root
                await _publishingService.PublishStagedReleaseContent();


                await UpdateStage(scheduled, Complete);
                    
                foreach (var releaseStatus in scheduled)
                {
                    try
                    {
                        await _publishingCompletionService.CompletePublishingIfAllStagesComplete(
                            releaseId: releaseStatus.ReleaseId, 
                            releaseStatusId: releaseStatus.Id,
                            DateTime.UtcNow);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Exception occured while executing {FunctionName}",
                            executionContext.FunctionName);
                        await UpdateStage(releaseStatus, Failed,
                            new ReleasePublishingStatusLogMessage($"Exception in publishing stage: {e.Message}"));
                    }
                }
            }

            logger.LogInformation(
                "{0} completed. {1}",
                executionContext.FunctionName,
                timer.FormatNextOccurrences(1));
        }

        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleases()
        {
            return await _releasePublishingStatusService.GetWherePublishingDueTodayWithStages(
                content: ReleasePublishingStatusContentStage.Complete,
                publishing: Scheduled);
        }

        private async Task UpdateStage(IEnumerable<ReleasePublishingStatus> releaseStatuses, ReleasePublishingStatusPublishingStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            foreach (var releaseStatus in releaseStatuses)
            {
                await UpdateStage(releaseStatus, stage, logMessage);
            }
        }

        private async Task UpdateStage(ReleasePublishingStatus releasePublishingStatus, ReleasePublishingStatusPublishingStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await _releasePublishingStatusService.UpdatePublishingStageAsync(releasePublishingStatus.ReleaseId, releasePublishingStatus.Id, stage,
                logMessage);
        }
    }
}
