using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class StageScheduledReleasesFunction(
        ILogger<StageScheduledReleasesFunction> logger,
        IOptions<AppOptions> appOptions,
        TimeProvider timeProvider,
        IQueueService queueService,
        IReleasePublishingStatusService releasePublishingStatusService)
    {
        private readonly AppOptions _appOptions = appOptions.Value;

        /// <summary>
        /// Azure function which triggers publishing files and staging content for all release versions that are scheduled to
        /// be published later during the day. This operates on a schedule which by default occurs at midnight every
        /// night.
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="context"></param>
        [Function(nameof(StageScheduledReleases))]
        public async Task StageScheduledReleases(
            [TimerTrigger("%App:PublishReleasesCronSchedule%")] TimerInfo timer,
            FunctionContext context)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

            // Get the next scheduled publishing time using the cron expression of the PublishScheduledReleases function
                var nextScheduledPublishingTime = CronExpressionUtil.GetNextOccurrence(
                    cronExpression: _appOptions.PublishReleaseContentCronSchedule,
                    from: timeProvider.GetUtcNow(),
                    timeZoneInfo: timeProvider.LocalTimeZone);

            // Fetch releases scheduled for publishing before or on the next run time
            var releasesToBeStaged = await releasePublishingStatusService
                .GetScheduledReleasesForPublishingRelativeToDate(
                    DateComparison.BeforeOrOn,
                    nextScheduledPublishingTime!.Value);

            await QueueReleaseFilesAndContentTasks(releasesToBeStaged);

            var stagedReleaseVersionIds = releasesToBeStaged.Select(key => key.ReleaseVersionId).ToArray();

            logger.LogInformation("{FunctionName} completed. Queued tasks for release versions: [{ReleaseVersionIds}]",
                context.FunctionDefinition.Name,
                stagedReleaseVersionIds.JoinToString(','));
        }

        /// <summary>
        /// Azure function to manually trigger the publishing/staging of releaseVersion(s) files/content. This can be
        /// done for either all releaseVersions that are scheduled to be published today, or an array of
        /// releaseVersions that are due to be published today or in the future. This is triggered manually by an HTTP
        /// post request, and is disabled in production environments. More info in the Publisher README.md.
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

            var utcNow = timeProvider.GetUtcNow();
            var startOfToday = new DateTimeOffset(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, TimeSpan.Zero);
            var endOfToday = startOfToday.AddDays(1).AddTicks(-1);

            var releasesToBeStaged = releaseVersionIds?.Length > 0
                ? await releasePublishingStatusService.GetScheduledReleasesForPublishingRelativeToDate(
                    DateComparison.After, startOfToday)
                : await releasePublishingStatusService.GetScheduledReleasesForPublishingRelativeToDate(
                    DateComparison.BeforeOrOn, endOfToday);

            var selectedReleasesToBeStaged = releaseVersionIds?.Length > 0
                ? releasesToBeStaged
                    .Where(key => releaseVersionIds.Contains(key.ReleaseVersionId))
                    .ToList()
                : releasesToBeStaged;

            await QueueReleaseFilesAndContentTasks(selectedReleasesToBeStaged);

            var stagedReleaseVersionIds = selectedReleasesToBeStaged.Select(key => key.ReleaseVersionId).ToArray();

            logger.LogInformation("{FunctionName} completed. Queued tasks for release versions: [{ReleaseVersionIds}]",
                context.FunctionDefinition.Name,
                stagedReleaseVersionIds.JoinToString(','));

            return new ManualTriggerResponse(stagedReleaseVersionIds);
        }

        private async Task QueueReleaseFilesAndContentTasks(IReadOnlyList<ReleasePublishingKey> scheduled)
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

        // ReSharper disable once ClassNeverInstantiated.Local
        private record ManualTriggerRequest(Guid[] ReleaseVersionIds);

        public record ManualTriggerResponse(Guid[] ReleaseVersionIds);
    }
}
