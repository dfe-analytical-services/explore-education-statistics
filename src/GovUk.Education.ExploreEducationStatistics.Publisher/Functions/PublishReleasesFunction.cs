using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusStates;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishReleasesFunction
    {
        private readonly IQueueService _queueService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;

        public PublishReleasesFunction(IQueueService queueService, IReleasePublishingStatusService releasePublishingStatusService)
        {
            _queueService = queueService;
            _releasePublishingStatusService = releasePublishingStatusService;
        }

        /// <summary>
        /// Azure function which triggers tasks for all Releases that are scheduled to be published later during the day.
        /// </summary>
        /// <remarks>
        /// Triggers publishing files and statistics data.
        /// </remarks>
        /// <param name="timer"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        [FunctionName("PublishReleases")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishReleases([TimerTrigger("%PublishReleasesCronSchedule%")]
            TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered at: {1}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            await PublishReleases();

            logger.LogInformation(
                "{0} completed. {1}",
                executionContext.FunctionName,
                timer.FormatNextOccurrences(1));
        }

        private async Task PublishReleases()
        {
            var scheduled = (await QueryScheduledReleases()).Select(status => (status.ReleaseId, status.Id)).ToList();
            if (scheduled.Any())
            {
                foreach (var (releaseId, releaseStatusId) in scheduled)
                {
                    await _releasePublishingStatusService.UpdateStateAsync(releaseId, releaseStatusId,
                        ScheduledReleaseStartedState);
                }

                await _queueService.QueuePublishReleaseFilesMessageAsync(scheduled);
            }
        }

        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleases()
        {
            return await _releasePublishingStatusService.GetWherePublishingDueTodayWithStages(
                overall: ReleasePublishingStatusOverallStage.Scheduled);
        }
    }
}
