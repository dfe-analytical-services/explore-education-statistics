using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class PublishScheduledReleasesFunction(
    ILogger<PublishScheduledReleasesFunction> logger,
    IReleasePublishingStatusService releasePublishingStatusService,
    IPublishingCompletionService publishingCompletionService
)
{
    /// <summary>
    /// Azure function which calls the <see cref="IPublishingCompletionService"/> in order to complete the publishing
    /// process for release versions that are scheduled to be published.
    /// </summary>
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

        var releasesReadyForPublishing = await releasePublishingStatusService.GetScheduledReleasesReadyForPublishing();

        // Complete publishing of these release versions
        await publishingCompletionService.CompletePublishingIfAllPriorStagesComplete(releasesReadyForPublishing);

        logger.LogInformation(
            "{FunctionName} completed. Published release versions [{ReleaseVersionIds}].",
            context.FunctionDefinition.Name,
            releasesReadyForPublishing.ToReleaseVersionIdsString()
        );
    }

    /// <summary>
    /// Azure function which calls the <see cref="IPublishingCompletionService"/> in order to complete the publishing
    /// process for release versions that are scheduled to be published.
    /// </summary>
    /// <remarks>
    /// This function is manually triggered by an HTTP POST, and is disabled by default in production environments.
    /// </remarks>
    /// <param name="request">
    /// An optional JSON request body with a "ReleaseVersionIds" array can be included in the POST request to limit
    /// the scope of the Function to only operate upon the provided release version id's.
    /// </param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Function(nameof(PublishStagedReleaseVersionContentImmediately))]
    public async Task<ActionResult<ManualTriggerResponse>> PublishStagedReleaseVersionContentImmediately(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var releaseVersionIds = (await request.GetJsonBody<ManualTriggerRequest>())?.ReleaseVersionIds;

        var releasesReadyForPublishing = await releasePublishingStatusService.GetScheduledReleasesReadyForPublishing();

        var selectedReleasesToPublish =
            releaseVersionIds?.Length > 0
                ? releasesReadyForPublishing.Where(key => releaseVersionIds.Contains(key.ReleaseVersionId)).ToList()
                : releasesReadyForPublishing;

        // Complete publishing of these release versions
        await publishingCompletionService.CompletePublishingIfAllPriorStagesComplete(selectedReleasesToPublish);

        logger.LogInformation(
            "{FunctionName} completed. Published release versions [{ReleaseVersionIds}]",
            context.FunctionDefinition.Name,
            selectedReleasesToPublish.ToReleaseVersionIdsString()
        );

        return new ManualTriggerResponse(selectedReleasesToPublish.ToReleaseVersionIds());
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record ManualTriggerRequest(Guid[] ReleaseVersionIds);

    public record ManualTriggerResponse(Guid[] ReleaseVersionIds);
}
