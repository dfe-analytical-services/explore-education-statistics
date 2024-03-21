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
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusStates;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class StageScheduledReleasesFunction(
        ILogger<StageScheduledReleasesFunction> logger,
        IQueueService queueService,
        IReleasePublishingStatusService releasePublishingStatusService)
    {
        /// <summary>
        /// Azure function which triggers publishing files and staging content for all release versions that are scheduled to
        /// be published later during the day. This operates on a schedule which by default occurs at midnight every
        /// night.
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="context"></param>
        [Function("StageScheduledReleases")]
        public async Task StageScheduledReleases(
            [TimerTrigger("%AppSettings:PublishReleasesCronSchedule%")] TimerInfo timer,
            FunctionContext context)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

            await PublishReleaseFilesAndStageContent((await QueryScheduledReleasesForToday()).ToArray());

            logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
        }

        /// <summary>
        /// Azure function which triggers publishing files and staging content for all release versions that are scheduled to
        /// be published later during the day. This is triggered manually by an HTTP post request, and is disabled in
        /// production environments.
        /// </summary>
        /// <param name="request">
        /// An optional JSON request body with a "ReleaseVersionIds" array can be included in the POST request to limit
        /// the scope of the Function to only the provided release version id's.
        /// </param>
        /// <param name="context"></param>
        [Function("StageScheduledReleaseVersionsImmediately")]
        public async Task<ActionResult<ManualTriggerResponse>> StageScheduledReleaseVersionsImmediately(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequest request,
            FunctionContext context)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

            var releaseVersionIds = (await request.GetJsonBody<ManualTriggerRequest>())?.ReleaseVersionIds;

            var scheduled = releaseVersionIds?.Length > 0
                ? (await QueryScheduledReleasesForTodayOrFuture())
                .Where(releaseStatus => releaseVersionIds.Contains(releaseStatus.ReleaseVersionId))
                : await QueryScheduledReleasesForToday();

            var stagingReleaseVersionIds = await PublishReleaseFilesAndStageContent(scheduled.ToArray());

            logger.LogInformation("{FunctionName} completed. Staged release versions [{ReleaseVersionIds}]",
                context.FunctionDefinition.Name,
                stagingReleaseVersionIds.JoinToString(','));

            return new ManualTriggerResponse(stagingReleaseVersionIds);
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
                    releasePublishingStatusService.UpdateStateAsync(
                        releaseVersionId: releaseStatus.ReleaseVersionId,
                        releaseStatusId: releaseStatus.Id,
                        ScheduledReleaseStartedState));

            var scheduledIds = scheduled
                .Select(releaseStatus => (releaseStatus.ReleaseVersionId, ReleaseStatusId: releaseStatus.Id));

            await queueService.QueuePublishReleaseFilesMessage(scheduledIds);
            await queueService.QueueGenerateStagedReleaseContentMessage(scheduledIds);

            return scheduled
                .Select(releaseStatus => releaseStatus.ReleaseVersionId)
                .ToArray();
        }

        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleasesForToday()
        {
            return await releasePublishingStatusService
                .GetWherePublishingDueTodayWithStages(
                    overall: ReleasePublishingStatusOverallStage.Scheduled);
        }

        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleasesForTodayOrFuture()
        {
            return await releasePublishingStatusService
                .GetWherePublishingDueTodayOrInFutureWithStages(
                    overall: ReleasePublishingStatusOverallStage.Scheduled);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private record ManualTriggerRequest(Guid[] ReleaseVersionIds);

        public record ManualTriggerResponse(Guid[] ReleaseVersionIds);
    }
}
