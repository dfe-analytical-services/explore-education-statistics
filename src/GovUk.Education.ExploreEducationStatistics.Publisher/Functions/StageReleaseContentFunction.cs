using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Publisher.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusContentStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class StageReleaseContentFunction(
    ILogger<StageReleaseContentFunction> logger,
    IOptions<AppOptions> appOptions,
    TimeProvider timeProvider,
    IContentService contentService,
    IReleasePublishingStatusService releasePublishingStatusService
)
{
    private readonly AppOptions _appOptions = appOptions.Value;

    /// <summary>
    /// Azure function which generates the content for a Release into a staging directory.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Function("StageReleaseContent")]
    public async Task StageReleaseContent(
        [QueueTrigger(StageReleaseContentQueue)] StageReleaseContentMessage message,
        FunctionContext context
    )
    {
        logger.LogInformation(
            "{FunctionName} triggered: {Message}",
            context.FunctionDefinition.Name,
            message
        );

        await UpdateContentStage(message, Started);

        var now = timeProvider.GetUtcNow();
        var timeZone = timeProvider.LocalTimeZone; // UTC or the time zone in WEBSITE_TIME_ZONE if specified

        try
        {
            var nextScheduledPublishingTime =
                CronExpressionUtil.GetNextOccurrence(
                    cronExpression: _appOptions.PublishScheduledReleasesFunctionCronSchedule,
                    from: now,
                    timeZone
                )
                ?? throw new CronNoFutureOccurrenceException(
                    cronExpression: _appOptions.PublishScheduledReleasesFunctionCronSchedule,
                    from: now,
                    timeZone
                );

            await contentService.UpdateContentStaged(
                expectedPublishDate: nextScheduledPublishingTime.UtcDateTime,
                releaseVersionIds: message.ReleasePublishingKeys.ToReleaseVersionIds()
            );
            await UpdateContentStage(message, Scheduled);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Exception occured while executing {FunctionName}",
                context.FunctionDefinition.Name
            );

            await UpdateContentStage(
                message,
                Failed,
                new ReleasePublishingStatusLogMessage($"Exception in content stage: {e.Message}")
            );
        }

        logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
    }

    private async Task UpdateContentStage(
        StageReleaseContentMessage message,
        ReleasePublishingStatusContentStage stage,
        ReleasePublishingStatusLogMessage? logMessage = null
    )
    {
        await message
            .ReleasePublishingKeys.ToAsyncEnumerable()
            .ForEachAwaitAsync(async key =>
            {
                await releasePublishingStatusService.UpdateContentStage(key, stage, logMessage);
            });
    }
}
