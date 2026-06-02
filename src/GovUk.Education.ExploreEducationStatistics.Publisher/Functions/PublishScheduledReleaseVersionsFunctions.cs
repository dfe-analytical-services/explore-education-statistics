using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class PublishScheduledReleaseVersionsFunctions(
    ILogger<PublishScheduledReleaseVersionsFunctions> logger,
    IReleasePublishingStatusService releasePublishingStatusService,
    IPublishingCompletionService publishingCompletionService
)
{
    /// <summary>
    /// Azure Function that publishes scheduled release versions.
    /// It uses <see cref="IPublishingCompletionService"/> to mark release versions as publicly accessible and to
    /// execute tasks required to complete the publishing process.
    /// </summary>
    /// <remarks>
    /// Triggered by a cron schedule that executes daily at 09:30:00 in the Production environment.
    /// </remarks>
    [Function(nameof(PublishScheduledReleaseVersions))]
    public async Task PublishScheduledReleaseVersions(
        [TimerTrigger("%App:PublishScheduledReleaseVersionsFunctionCronSchedule%")] TimerInfo timer,
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var releaseVersionsReadyForPublishing =
            await releasePublishingStatusService.GetScheduledReleasesReadyForPublishing();

        // Complete publishing of these release versions
        await publishingCompletionService.CompletePublishing(releaseVersionsReadyForPublishing);

        logger.LogInformation(
            "{FunctionName} completed. Published release versions [{ReleaseVersionIds}].",
            context.FunctionDefinition.Name,
            releaseVersionsReadyForPublishing.ToReleaseVersionIdsString()
        );
    }

    /// <summary>
    /// HTTP-triggered function to immediately publish scheduled release versions.
    /// Intended for use by manual and automated testing to avoid waiting for the scheduled trigger.
    /// It uses <see cref="IPublishingCompletionService"/> to mark release versions as publicly accessible and to
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
    public async Task<IActionResult> PublishScheduledReleaseVersionsNow(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var releaseVersionIds = (await request.GetJsonBody<ManualTriggerRequest>())?.ReleaseVersionIds;

        var releaseVersionsReadyForPublishing =
            await releasePublishingStatusService.GetScheduledReleasesReadyForPublishing();

        var selectedReleaseVersionsToPublish =
            releaseVersionIds?.Length > 0
                ? releaseVersionsReadyForPublishing
                    .Where(key => releaseVersionIds.Contains(key.ReleaseVersionId))
                    .ToList()
                : releaseVersionsReadyForPublishing;

        // Complete publishing of these release versions
        await publishingCompletionService.CompletePublishing(selectedReleaseVersionsToPublish);

        logger.LogInformation(
            "{FunctionName} completed. Published release versions [{ReleaseVersionIds}]",
            context.FunctionDefinition.Name,
            selectedReleaseVersionsToPublish.ToReleaseVersionIdsString()
        );

        return new OkObjectResult(new ManualTriggerResponse(selectedReleaseVersionsToPublish.ToReleaseVersionIds()));
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record ManualTriggerRequest(Guid[] ReleaseVersionIds);

    private record ManualTriggerResponse(Guid[] ReleaseVersionIds);
}
