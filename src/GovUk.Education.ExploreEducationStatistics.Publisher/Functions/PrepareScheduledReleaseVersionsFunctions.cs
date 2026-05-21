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

public class PrepareScheduledReleaseVersionsFunctions(
    ILogger<PrepareScheduledReleaseVersionsFunctions> logger,
    IOptions<AppOptions> appOptions,
    TimeProvider timeProvider,
    IQueueService queueService,
    IReleasePublishingStatusService releasePublishingStatusService
)
{
    private readonly AppOptions _appOptions = appOptions.Value;

    /// <summary>
    /// Azure function that prepares release versions for publishing by triggering file copying for all release versions
    /// scheduled to be published later that day.
    /// </summary>
    /// <remarks>
    /// Triggered by a cron schedule that executes daily at 00:05:00 in the Production environment.
    /// </remarks>
    [Function(nameof(PrepareScheduledReleaseVersions))]
    public async Task PrepareScheduledReleaseVersions(
        [TimerTrigger("%App:PrepareScheduledReleaseVersionsFunctionCronSchedule%")] TimerInfo timer,
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var now = timeProvider.GetUtcNow();
        var timeZone = timeProvider.LocalTimeZone; // UTC or the time zone in WEBSITE_TIME_ZONE if specified

        // Get the next scheduled publishing time using the cron expression of the PublishScheduledReleaseVersions function
        var nextScheduledPublishingTime =
            CronExpressionUtil.GetNextOccurrence(
                cronExpression: _appOptions.PublishScheduledReleaseVersionsFunctionCronSchedule,
                from: now,
                timeZone
            )
            ?? throw new CronNoFutureOccurrenceException(
                cronExpression: _appOptions.PublishScheduledReleaseVersionsFunctionCronSchedule,
                from: now,
                timeZone
            );

        // Fetch release versions scheduled for publishing before or on the next run time
        var scheduledReleaseVersions =
            await releasePublishingStatusService.GetScheduledReleasesForPublishingRelativeToDate(
                DateComparison.BeforeOrOn,
                nextScheduledPublishingTime
            );

        await QueueReleaseFilesTask(scheduledReleaseVersions);

        logger.LogInformation(
            "{FunctionName} completed. Queued tasks for release versions: [{ReleaseVersionIds}]",
            context.FunctionDefinition.Name,
            scheduledReleaseVersions.ToReleaseVersionIdsString()
        );
    }

    /// <summary>
    /// HTTP-triggered function to immediately prepare release versions for publishing by triggering file copying for
    /// all release versions scheduled to be published later that day.
    /// Intended for use by manual and automated testing to avoid waiting for the scheduled trigger.
    /// </summary>
    /// <remarks>
    /// This function is manually triggered by an HTTP POST and is disabled by default in production.
    /// It mirrors the behaviour of <see cref="PrepareScheduledReleaseVersions"/>.
    /// For more info see the Publisher's README.
    /// </remarks>
    /// <param name="request">
    /// An optional JSON request body with a "ReleaseVersionIds" array can be included in the POST request to limit
    /// the scope of the Function to only the provided release version id's.
    /// </param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Function(nameof(PrepareScheduledReleaseVersionsNow))]
    public async Task<ActionResult<ManualTriggerResponse>> PrepareScheduledReleaseVersionsNow(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var releaseVersionIds = (await request.GetJsonBody<ManualTriggerRequest>())?.ReleaseVersionIds;

        var now = timeProvider.GetLocalNow();
        var startOfToday = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);

        var scheduledReleaseVersions =
            releaseVersionIds?.Length > 0
                ? await releasePublishingStatusService.GetScheduledReleasesForPublishingRelativeToDate(
                    DateComparison.AfterOrOn,
                    startOfToday
                )
                : await releasePublishingStatusService.GetScheduledReleasesForPublishingRelativeToDate(
                    DateComparison.Before,
                    startOfToday.AddDays(1)
                );

        var selectedReleaseVersions =
            releaseVersionIds?.Length > 0
                ? scheduledReleaseVersions.Where(key => releaseVersionIds.Contains(key.ReleaseVersionId)).ToList()
                : scheduledReleaseVersions;

        await QueueReleaseFilesTask(selectedReleaseVersions);

        logger.LogInformation(
            "{FunctionName} completed. Queued tasks for release versions: [{ReleaseVersionIds}]",
            context.FunctionDefinition.Name,
            selectedReleaseVersions.ToReleaseVersionIdsString()
        );

        return new ManualTriggerResponse(selectedReleaseVersions.ToReleaseVersionIds());
    }

    private async Task QueueReleaseFilesTask(IReadOnlyList<ReleasePublishingKey> releasePublishingKeys)
    {
        if (!releasePublishingKeys.Any())
        {
            return;
        }

        foreach (var key in releasePublishingKeys)
        {
            await releasePublishingStatusService.UpdateState(
                key,
                ReleasePublishingStatusStates.ScheduledReleaseStartedState
            );
        }

        await queueService.QueueReleaseFilesForScheduledPublishing(releasePublishingKeys);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record ManualTriggerRequest(Guid[] ReleaseVersionIds);

    public record ManualTriggerResponse(Guid[] ReleaseVersionIds);
}
