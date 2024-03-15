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
        /// Azure function which publishes the content for a release version at a scheduled time by moving it from a staging
        /// directory.
        /// </summary>
        /// <remarks>
        /// It will then call PublishingCompletionService in order to complete the publishing process for that release version.
        /// </remarks>
        /// <param name="timer"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("PublishStagedReleaseContent")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishScheduledReleases([TimerTrigger("%PublishReleaseContentCronSchedule%")] TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{FunctionName} triggered at: {DateTime}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            var publishedReleaseVersionIds =
                await PublishScheduledReleases((await QueryScheduledReleasesForToday()).ToArray());

            logger.LogInformation(
                "{FunctionName} completed.  Published release versions [{ReleaseVersionIds}].  " +
                "Will be scheduled again to run at {NextDateTime}",
                executionContext.FunctionName,
                publishedReleaseVersionIds.JoinToString(','),
                timer.FormatNextOccurrences(1));
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
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("PublishStagedReleaseVersionContentImmediately")]
        // ReSharper disable once UnusedMember.Global
        public async Task<ActionResult<ManualTriggerResponse>> PublishStagedReleaseVersionContentImmediately(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequest request,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{FunctionName} triggered at: {DateTime}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            var releaseVersionIds = (await request.GetJsonBody<ManualTriggerRequest>())?.ReleaseVersionIds;

            var scheduled = releaseVersionIds?.Length > 0
                ? (await QueryScheduledReleasesForTodayOrFuture())
                .Where(releaseStatus => releaseVersionIds.Contains(releaseStatus.ReleaseVersionId))
                : await QueryScheduledReleasesForToday();

            var publishedReleaseVersionIds = await PublishScheduledReleases(scheduled.ToArray());

            logger.LogInformation("{FunctionName} completed. Published release versions [{ReleaseVersionIds}]",
                executionContext.FunctionName,
                publishedReleaseVersionIds.JoinToString(','));

            return new ManualTriggerResponse(publishedReleaseVersionIds);
        }

        private async Task<Guid[]> PublishScheduledReleases(ReleasePublishingStatus[] scheduled)
        {
            if (!scheduled.Any())
            {
                return Array.Empty<Guid>();
            }

            await _publishingService.PublishStagedReleaseContent();

            await scheduled
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async message =>
                    await UpdateContentStage(message, ReleasePublishingStatusContentStage.Complete));

            // Finalise publishing of these releases
            await _publishingCompletionService.CompletePublishingIfAllPriorStagesComplete(
                scheduled.Select(status => (status.ReleaseVersionId, ReleaseStatusId: status.Id)));

            return scheduled
                .Select(releaseStatus => releaseStatus.ReleaseVersionId)
                .ToArray();
        }

        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleasesForToday()
        {
            return await _releasePublishingStatusService.GetWherePublishingDueTodayWithStages(
                content: ReleasePublishingStatusContentStage.Scheduled,
                files: ReleasePublishingStatusFilesStage.Complete,
                publishing: ReleasePublishingStatusPublishingStage.Scheduled);
        }

        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleasesForTodayOrFuture()
        {
            return await _releasePublishingStatusService.GetWherePublishingDueTodayOrInFutureWithStages(
                content: ReleasePublishingStatusContentStage.Scheduled,
                files: ReleasePublishingStatusFilesStage.Complete,
                publishing: ReleasePublishingStatusPublishingStage.Scheduled);
        }

        private async Task UpdateContentStage(
            ReleasePublishingStatus status,
            ReleasePublishingStatusContentStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await _releasePublishingStatusService.UpdateContentStageAsync(
                releaseVersionId: status.ReleaseVersionId,
                releaseStatusId: status.Id,
                stage,
                logMessage);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private record ManualTriggerRequest(Guid[] ReleaseVersionIds);

        public record ManualTriggerResponse(Guid[] ReleaseVersionIds);
    }
}
