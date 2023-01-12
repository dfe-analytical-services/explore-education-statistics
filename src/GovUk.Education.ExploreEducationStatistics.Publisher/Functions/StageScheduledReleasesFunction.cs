using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusStates;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class StageScheduledReleasesFunction
    {
        private readonly IQueueService _queueService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;

        public StageScheduledReleasesFunction(IQueueService queueService, IReleasePublishingStatusService releasePublishingStatusService)
        {
            _queueService = queueService;
            _releasePublishingStatusService = releasePublishingStatusService;
        }

        /// <summary>
        /// Azure function which triggers publishing files and staging content for all Releases that are scheduled to
        /// be published later during the day. This operates on a schedule which by default occurs at midnight every
        /// night.
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        [FunctionName("StageScheduledReleases")]
        // ReSharper disable once UnusedMember.Global
        public async Task StageScheduledReleases([TimerTrigger("%PublishReleasesCronSchedule%")]
            TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered at: {1}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            await PublishReleaseFilesAndStageContent();

            logger.LogInformation(
                "{0} completed. {1}",
                executionContext.FunctionName,
                timer.FormatNextOccurrences(1));
        }
        
        /// <summary>
        /// Azure function which triggers publishing files and staging content for all Releases that are scheduled to
        /// be published later during the day. This is triggered manually by an HTTP post request, and is disabled in
        /// production environments.
        /// </summary>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        [FunctionName("StageScheduledReleasesImmediately")]
        // ReSharper disable once UnusedMember.Global
        public async Task ManuallyStageScheduledReleases(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered at: {1}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            await PublishReleaseFilesAndStageContent();

            logger.LogInformation("{0} completed.", executionContext.FunctionName);
        }

        private async Task PublishReleaseFilesAndStageContent()
        {
            var scheduled = (await QueryScheduledReleases()).Select(status => (status.ReleaseId, status.Id)).ToList();
            if (scheduled.Any())
            {
                foreach (var (releaseId, releaseStatusId) in scheduled)
                {
                    await _releasePublishingStatusService.UpdateStateAsync(releaseId, releaseStatusId,
                        ScheduledReleaseStartedState);
                }

                await _queueService.QueuePublishReleaseFilesMessage(scheduled);
                await _queueService.QueueGenerateStagedReleaseContentMessage(scheduled);
            }
        }

        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleases()
        {
            return await _releasePublishingStatusService.GetWherePublishingDueTodayWithStages(
                overall: ReleasePublishingStatusOverallStage.Scheduled);
        }
    }
}
