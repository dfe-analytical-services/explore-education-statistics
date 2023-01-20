#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            logger.LogInformation("{FunctionName} triggered at: {DateTime}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            await PublishReleaseFilesAndStageContent((await QueryScheduledReleasesForToday()).ToArray());

            logger.LogInformation(
                "{FunctionName} completed.  Will be scheduled again to run at {NextDateTime}",
                executionContext.FunctionName,
                timer.FormatNextOccurrences(1));
        }

        /// <summary>
        /// Azure function which triggers publishing files and staging content for all Releases that are scheduled to
        /// be published later during the day. This is triggered manually by an HTTP post request, and is disabled in
        /// production environments.
        /// </summary>
        /// <param name="request">
        /// An optional JSON request body with a "ReleaseIds" array can be included in the POST request to limit the
        /// scope of the Function to only the provided Release Ids.
        /// </param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        [FunctionName("StageScheduledReleasesImmediately")]
        // ReSharper disable once UnusedMember.Global
        public async Task<ActionResult<ManualTriggerResponse>> StageScheduledReleasesImmediately(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequest request,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{FunctionName} triggered at: {DateTime}",
                executionContext.FunctionName,
                DateTime.UtcNow);       

            var releaseIds = (await request.GetJsonBody<ManualTriggerRequest>())?.ReleaseIds;
            
            var scheduled = releaseIds?.Length > 0 
                ? (await QueryScheduledReleasesForTodayOrFuture())
                .Where(releaseStatus => releaseIds.Contains(releaseStatus.ReleaseId))
                : await QueryScheduledReleasesForToday();
            
            var stagingReleaseIds = await PublishReleaseFilesAndStageContent(scheduled.ToArray());

            logger.LogInformation("{FunctionName} completed. Staged Releases [{ReleaseIds}]", 
                executionContext.FunctionName,
                stagingReleaseIds.JoinToString(','));

            return new ManualTriggerResponse(stagingReleaseIds);
        }

        private async Task<Guid[]> PublishReleaseFilesAndStageContent(ReleasePublishingStatus[] scheduled)
        {
            if (!scheduled.Any())
            {
                return Array.Empty<Guid>();
            }

            await scheduled
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(releaseStatus => 
                    _releasePublishingStatusService.UpdateStateAsync(
                        releaseStatus.ReleaseId, 
                        releaseStatus.Id, 
                        ScheduledReleaseStartedState));

            var scheduledIds = scheduled
                .Select(releaseStatus => (releaseStatus.ReleaseId, releaseStatus.Id));

            await _queueService.QueuePublishReleaseFilesMessage(scheduledIds);
            await _queueService.QueueGenerateStagedReleaseContentMessage(scheduledIds);
            
            return scheduled
                .Select(releaseStatus => releaseStatus.ReleaseId)
                .ToArray();
        }

        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleasesForToday()
        {
            return await _releasePublishingStatusService
                .GetWherePublishingDueTodayWithStages(
                    overall: ReleasePublishingStatusOverallStage.Scheduled);
        }
        
        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleasesForTodayOrFuture()
        {
            return await _releasePublishingStatusService
                .GetWherePublishingDueTodayOrInFutureWithStages(
                    overall: ReleasePublishingStatusOverallStage.Scheduled);
        }
        
        // ReSharper disable once ClassNeverInstantiated.Local
        private record ManualTriggerRequest(Guid[] ReleaseIds);
        
        public record ManualTriggerResponse(Guid[] ReleaseIds);
    }
}
