using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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

            await PublishReleaseFilesAndStageContent(await QueryScheduledReleasesForToday());

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
            FunctionContext context)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

            var releaseVersionIds = (await request.GetJsonBody<ManualTriggerRequest>())?.ReleaseVersionIds;

            var scheduled = releaseVersionIds?.Length > 0
                ? await QueryScheduledReleasesForTodayOrFuture(releaseVersionIds)
                : await QueryScheduledReleasesForToday();

            await PublishReleaseFilesAndStageContent(scheduled);

            var stagedReleaseVersionIds = scheduled.Select(key => key.ReleaseVersionId).ToArray();

            logger.LogInformation("{FunctionName} completed. Staged release versions [{ReleaseVersionIds}]",
                context.FunctionDefinition.Name,
                stagedReleaseVersionIds.JoinToString(','));

            return new ManualTriggerResponse(stagedReleaseVersionIds);
        }

        private async Task PublishReleaseFilesAndStageContent(IReadOnlyList<ReleasePublishingKey> scheduled)
        {
            if (!scheduled.Any())
            {
                return;
            }

            await scheduled
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async key =>
                    await releasePublishingStatusService.UpdateState(key,
                        ReleasePublishingStatusStates.ScheduledReleaseStartedState));

            await queueService.QueuePublishReleaseFilesMessages(scheduled);
            await queueService.QueueStageReleaseContentMessages(scheduled);
        }

        private async Task<IReadOnlyList<ReleasePublishingKey>> QueryScheduledReleasesForToday() // @MarkFix remove
        {
            return await releasePublishingStatusService.GetWherePublishingDueTodayWithStages(
                overall: ReleasePublishingStatusOverallStage.Scheduled);
        }

        private async Task<IReadOnlyList<ReleasePublishingKey>> QueryScheduledReleasesForTodayOrFuture(
            IReadOnlyList<Guid> releaseVersionIds)
        {
            return await releasePublishingStatusService.GetWherePublishingDueTodayOrInFutureWithStages(
                releaseVersionIds,
                overall: ReleasePublishingStatusOverallStage.Scheduled);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private record ManualTriggerRequest(Guid[] ReleaseVersionIds);

        public record ManualTriggerResponse(Guid[] ReleaseVersionIds);
    }
}
