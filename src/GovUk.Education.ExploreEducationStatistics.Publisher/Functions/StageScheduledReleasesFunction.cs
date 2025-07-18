using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Publisher.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

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
        [TimerTrigger("%App:StageScheduledReleasesFunctionCronSchedule%")] TimerInfo timer,
        FunctionContext context)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var now = timeProvider.GetUtcNow();
        var timeZone = timeProvider.LocalTimeZone; // UTC or the time zone in WEBSITE_TIME_ZONE if specified

        // Get the next scheduled publishing time using the cron expression of the PublishScheduledReleases function
        var nextScheduledPublishingTime = CronExpressionUtil.GetNextOccurrence(
            cronExpression: _appOptions.PublishScheduledReleasesFunctionCronSchedule,
            from: now,
            timeZone
        ) ?? throw new CronNoFutureOccurrenceException(
            cronExpression: _appOptions.PublishScheduledReleasesFunctionCronSchedule,
            from: now,
            timeZone);

        // Fetch releases scheduled for publishing before or on the next run time
        var releasesToBeStaged = await releasePublishingStatusService
            .GetScheduledReleasesForPublishingRelativeToDate(
                DateComparison.BeforeOrOn,
                nextScheduledPublishingTime);

        await QueueReleaseFilesAndContentTasks(releasesToBeStaged);

        logger.LogInformation("{FunctionName} completed. Queued tasks for release versions: [{ReleaseVersionIds}]",
            context.FunctionDefinition.Name,
            releasesToBeStaged.ToReleaseVersionIdsString());
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
    [Function(nameof(StageScheduledReleaseVersionsImmediately))]
    public async Task<ActionResult<ManualTriggerResponse>> StageScheduledReleaseVersionsImmediately(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
        FunctionContext context)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var releaseVersionIds = (await request.GetJsonBody<ManualTriggerRequest>())?.ReleaseVersionIds;

        var now = timeProvider.GetLocalNow();
        var startOfToday = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);

        var releasesToBeStaged = releaseVersionIds?.Length > 0
            ? await releasePublishingStatusService.GetScheduledReleasesForPublishingRelativeToDate(
                DateComparison.AfterOrOn,
                startOfToday)
            : await releasePublishingStatusService.GetScheduledReleasesForPublishingRelativeToDate(
                DateComparison.Before,
                startOfToday.AddDays(1));

        var selectedReleasesToBeStaged = releaseVersionIds?.Length > 0
            ? releasesToBeStaged
                .Where(key => releaseVersionIds.Contains(key.ReleaseVersionId))
                .ToList()
            : releasesToBeStaged;

        await QueueReleaseFilesAndContentTasks(selectedReleasesToBeStaged);

        logger.LogInformation("{FunctionName} completed. Queued tasks for release versions: [{ReleaseVersionIds}]",
            context.FunctionDefinition.Name,
            selectedReleasesToBeStaged.ToReleaseVersionIdsString());

        return new ManualTriggerResponse(selectedReleasesToBeStaged.ToReleaseVersionIds());
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
