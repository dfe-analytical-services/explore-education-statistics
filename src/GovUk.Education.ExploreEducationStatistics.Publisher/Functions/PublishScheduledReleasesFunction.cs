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
    public class PublishScheduledReleasesFunction(
        ILogger<PublishScheduledReleasesFunction> logger,
        IReleasePublishingStatusService releasePublishingStatusService,
        IPublishingService publishingService,
        IPublishingCompletionService publishingCompletionService)
    {
        /// <summary>
        /// Azure function which publishes the content for a release version at a scheduled time by moving it from a staging
        /// directory.
        /// </summary>
        /// <remarks>
        /// It will then call PublishingCompletionService in order to complete the publishing process for that release version.
        /// </remarks>
        /// <param name="timer"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Function("PublishStagedReleaseContent")]
        public async Task PublishScheduledReleases(
            [TimerTrigger("%AppSettings:PublishReleaseContentCronSchedule%")] TimerInfo timer,
            FunctionContext context)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

            var scheduled = await QueryScheduledReleasesForToday();

            await PublishScheduledReleases(scheduled);

            logger.LogInformation(
                "{FunctionName} completed. Published release versions [{ReleaseVersionIds}].",
                context.FunctionDefinition.Name,
                scheduled.Select(key => key.ReleaseVersionId).JoinToString(','));
        }

        /// <summary>
        /// Azure function which publishes the content for a release version immediately by moving it from a staging
        /// directory. This function is manually triggered by an HTTP POST, and is disabled by default in production
        /// environments. 
        /// </summary>
        /// <remarks>
        /// It will then call PublishingCompletionService in order to complete the publishing process for that release version.
        /// </remarks>
        /// <param name="request">
        /// An optional JSON request body with a "ReleaseVersionIds" array can be included in the POST request to limit
        /// the scope of the Function to only operate upon the provided release version id's.
        /// </param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Function("PublishStagedReleaseVersionContentImmediately")]
        public async Task<ActionResult<ManualTriggerResponse>> PublishStagedReleaseVersionContentImmediately(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
            FunctionContext context)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

            var releaseVersionIds = (await request.GetJsonBody<ManualTriggerRequest>())?.ReleaseVersionIds;

            var scheduled = releaseVersionIds?.Length > 0
                ? await QueryScheduledReleasesForTodayOrFuture(releaseVersionIds)
                : await QueryScheduledReleasesForToday();

            await PublishScheduledReleases(scheduled);

            var publishedReleaseVersionIds = scheduled.Select(key => key.ReleaseVersionId).ToArray();

            logger.LogInformation("{FunctionName} completed. Published release versions [{ReleaseVersionIds}]",
                context.FunctionDefinition.Name,
                publishedReleaseVersionIds.JoinToString(','));

            return new ManualTriggerResponse(publishedReleaseVersionIds);
        }

        private async Task PublishScheduledReleases(IReadOnlyList<ReleasePublishingKeyOld> scheduled)
        {
            if (!scheduled.Any())
            {
                return;
            }

            await publishingService.PublishStagedReleaseContent();

            await scheduled
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async keyOld =>
                {
                    var key = new ReleasePublishingKey(keyOld.ReleaseVersionId, keyOld.ReleaseStatusId);
                    await releasePublishingStatusService.UpdateContentStage(key,
                        ReleasePublishingStatusContentStage.Complete);
                });

            // Finalise publishing of these releases
            await publishingCompletionService.CompletePublishingIfAllPriorStagesComplete(scheduled);
        }

        private async Task<IReadOnlyList<ReleasePublishingKeyOld>> QueryScheduledReleasesForToday()
        {
            return await releasePublishingStatusService.GetWherePublishingDueTodayWithStages(
                content: ReleasePublishingStatusContentStage.Scheduled,
                files: ReleasePublishingStatusFilesStage.Complete,
                publishing: ReleasePublishingStatusPublishingStage.Scheduled);
        }

        private async Task<IReadOnlyList<ReleasePublishingKeyOld>> QueryScheduledReleasesForTodayOrFuture(
            IReadOnlyList<Guid> releaseVersionIds)
        {
            return await releasePublishingStatusService.GetWherePublishingDueTodayOrInFutureWithStages(
                releaseVersionIds,
                content: ReleasePublishingStatusContentStage.Scheduled,
                files: ReleasePublishingStatusFilesStage.Complete,
                publishing: ReleasePublishingStatusPublishingStage.Scheduled);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private record ManualTriggerRequest(Guid[] ReleaseVersionIds);

        public record ManualTriggerResponse(Guid[] ReleaseVersionIds);
    }
}
