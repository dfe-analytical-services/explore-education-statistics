using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class PublishScheduledReleasesFunction(
    ILogger<PublishScheduledReleasesFunction> logger,
    IReleasePublishingStatusService releasePublishingStatusService,
    IPublishingService publishingService,
    IPublishingCompletionService publishingCompletionService
)
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
    [Function(nameof(PublishScheduledReleases))]
    public async Task PublishScheduledReleases(
        [TimerTrigger("%App:PublishScheduledReleasesFunctionCronSchedule%")] TimerInfo timer,
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var releasesReadyForPublishing =
            await releasePublishingStatusService.GetScheduledReleasesReadyForPublishing();

        await PublishScheduledReleases(releasesReadyForPublishing);

        logger.LogInformation(
            "{FunctionName} completed. Published release versions [{ReleaseVersionIds}].",
            context.FunctionDefinition.Name,
            releasesReadyForPublishing.ToReleaseVersionIdsString()
        );
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
    [Function(nameof(PublishStagedReleaseVersionContentImmediately))]
    public async Task<
        ActionResult<ManualTriggerResponse>
    > PublishStagedReleaseVersionContentImmediately(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var releaseVersionIds = (
            await request.GetJsonBody<ManualTriggerRequest>()
        )?.ReleaseVersionIds;

        var releasesReadyForPublishing =
            await releasePublishingStatusService.GetScheduledReleasesReadyForPublishing();

        var selectedReleasesToPublish =
            releaseVersionIds?.Length > 0
                ? releasesReadyForPublishing
                    .Where(key => releaseVersionIds.Contains(key.ReleaseVersionId))
                    .ToList()
                : releasesReadyForPublishing;

        await PublishScheduledReleases(selectedReleasesToPublish);

        logger.LogInformation(
            "{FunctionName} completed. Published release versions [{ReleaseVersionIds}]",
            context.FunctionDefinition.Name,
            selectedReleasesToPublish.ToReleaseVersionIdsString()
        );

        return new ManualTriggerResponse(selectedReleasesToPublish.ToReleaseVersionIds());
    }

    private async Task PublishScheduledReleases(IReadOnlyList<ReleasePublishingKey> scheduled)
    {
        if (!scheduled.Any())
        {
            return;
        }

        await publishingService.PublishStagedReleaseContent();

        await scheduled
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async key =>
            {
                await releasePublishingStatusService.UpdateContentStage(
                    key,
                    ReleasePublishingStatusContentStage.Complete
                );
            });

        // Finalise publishing of these releases
        await publishingCompletionService.CompletePublishingIfAllPriorStagesComplete(scheduled);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record ManualTriggerRequest(Guid[] ReleaseVersionIds);

    public record ManualTriggerResponse(Guid[] ReleaseVersionIds);
}
