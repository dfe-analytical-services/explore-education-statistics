#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusPublishingStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishScheduledReleasesFunction
    {
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly IPublishingService _publishingService;
        private readonly IPublishingCompletionService _publishingCompletionService;

        public PublishScheduledReleasesFunction(
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
        public async Task PublishScheduledReleases([TimerTrigger("%PublishReleaseContentCronSchedule%")]
            TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{FunctionName} triggered at: {DateTime}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            var scheduled = (await QueryScheduledReleases()).ToArray();
            if (scheduled.Any())
            {
                // Move all cached releases in the staging directory of the public content container to the root
                await _publishingService.PublishStagedReleaseContent();

                // Finalise publishing of these releases
                await _publishingCompletionService.CompletePublishingIfAllPriorStagesComplete(
                    scheduled,
                    DateTime.UtcNow);
            }

            logger.LogInformation(
                "{FunctionName} completed. {Count}",
                executionContext.FunctionName,
                timer.FormatNextOccurrences(1));
        }

        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleases()
        {
            return await _releasePublishingStatusService.GetWherePublishingDueTodayWithStages(
                content: ReleasePublishingStatusContentStage.Complete,
                publishing: Scheduled);
        }
    }
}
