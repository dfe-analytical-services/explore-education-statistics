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
    /// Azure Function that publishes scheduled release versions.
    /// It uses <see cref="IPublishingCompletionService"/> to mark releases as publicly accessible and to
    /// execute tasks required to complete the publishing process.
    /// </summary>
    /// <remarks>
    /// Triggered by a cron schedule that executes daily at 09:30:00 in the Production environment.
    /// </remarks>
    [Function(nameof(PublishScheduledReleaseVersions))]
    public async Task PublishScheduledReleaseVersions(
        [TimerTrigger("%App:PublishScheduledReleasesFunctionCronSchedule%")] TimerInfo timer,
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var releasesReadyForPublishing = await releasePublishingStatusService.GetScheduledReleasesReadyForPublishing();

        // Complete publishing of these release versions
        await publishingCompletionService.CompletePublishing(releasesReadyForPublishing);

        logger.LogInformation(
            "{FunctionName} completed. Published release versions [{ReleaseVersionIds}].",
            context.FunctionDefinition.Name,
            releasesReadyForPublishing.ToReleaseVersionIdsString()
        );
    }

    /// <summary>
    /// HTTP-triggered function to immediately publish scheduled release versions.
    /// Intended for use by manual and automated testing to avoid waiting for the scheduled trigger.
    /// It uses <see cref="IPublishingCompletionService"/> to mark releases as publicly accessible and to
    /// execute tasks required to complete the publishing process.
    /// </summary>
    /// <remarks>
    /// This function is manually triggered by an HTTP POST and is disabled by default in production.
    /// It mirrors the behaviour of <see cref="PublishScheduledReleaseVersions"/>.
    /// For more info see the Publisher's README.
    /// </remarks>
    /// <param name="request">
    /// An optional JSON request body with a "ReleaseVersionIds" array can be included in the POST request to limit
    /// the scope of the Function to only operate upon the provided release version id's.
    /// </param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Function(nameof(PublishScheduledReleaseVersionsNow))]
    public async Task<ActionResult<ManualTriggerResponse>> PublishScheduledReleaseVersionsNow(
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
        await publishingCompletionService.CompletePublishing(selectedReleasesToPublish);

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
